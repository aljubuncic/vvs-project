using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Burgija.Data;
using Burgija.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Collections;
using Microsoft.AspNetCore.Http;
using Burgija.ViewModels;
using Burgija.Interfaces;
using Burgija.Tests;

namespace Burgija.Controllers
{
    /// <summary>
    /// Controller responsible for handling rental-related actions.
    /// </summary>
    public class RentController : Controller
    {
        ///Application database context
        private readonly IApplicationDbContext _context;
        private readonly ISessionService _sessionService;
        /// <summary>
        /// Initializes a new instance of the <see cref="RentController"/> class.
        /// </summary>
        /// <param name="context">The application database context.</param>
        public RentController(IApplicationDbContext context, ISessionService sessionService = null)
        {
            _context = context;
            _sessionService = sessionService;
        }



        public double CalculateDiscount(Tool tool, int code, List<Discount> discounts) {
            double toolPrice = tool.ToolType.Price;
            double discountPrice;
            for (int i = 0; i < discounts.Count; i++) {
                if (code == discounts[i].Id) {
                    if (DateTime.Now < discounts[i].StartOfDiscount || DateTime.Now > discounts[i].EndOfDiscount) {
                        throw new InvalidOperationException();
                    } else {
                        discountPrice = toolPrice * (1 - discounts[i].Percent / 100);
                        return discountPrice;
                    }
                }
            }
            throw new ArgumentException();
        }

        /// <summary>
        /// Displays the rental history for the registered user.
        /// </summary>
        /// <returns>The rental history view.</returns>
        [Authorize(Roles ="RegisteredUser")]
        public async Task<IActionResult> RentHistory()
        {
            // Extract the user ID from the claims associated with the current user
            var userId = Int32.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            // Query the database to retrieve rent history information
            var rentHistory = await _context.Rent
                // Filter by the current user's ID
                .Where(r => r.UserId == userId)
                // Join the Rent table with the Tool table using the ToolId
                .Join(_context.Tool, rent => rent.ToolId, tool => tool.Id, (rent, tool) => new { Rent = rent, Tool = tool })
                // Join the result with the ToolType table using the ToolTypeId from the Tool table
                .Join(_context.ToolType, rt => rt.Tool.ToolTypeId, toolType => toolType.Id, (rt, toolType) => new RentAndToolType(rt.Rent, toolType))
                // Convert the result to a List asynchronously
                .ToListAsync();

            // Return the rent history to the corresponding view
            return View(rentHistory);
        }

        /// <summary>
        /// Handles the HTTP POST request for obtaining a tool type.
        /// </summary>
        /// <param name="toolTypeId">The ID of the selected tool type.</param>
        /// <returns>A redirect to the 'Create' action with the tool type ID.</returns>
        [HttpPost]
        public IActionResult GetToolType(int? toolTypeId)
        {
  
            if (toolTypeId == null)
            {
                return NotFound();
            }

            //Store the toolType value in session
            HttpContext.Session.SetInt32("ToolType", (int)toolTypeId);

            //Redirect to the Create action with the tool type ID
            return RedirectToAction("Create", new { toolTypeId });
        }

        /// <summary>
        /// Displays a view for creating a new rental with details related to a specific tool type.
        /// </summary>
        /// <param name="toolTypeId">The ID of the tool type for which the rental is being created.</param>
        /// <returns>
        /// - If the tool type ID is null, returns a "NotFound" result.
        /// - If the tool type with the specified ID is not found, returns a "NotFound" result.
        /// - Otherwise, returns a view for creating a new rental with details related to the specified tool type.
        /// </returns>
        public async Task<IActionResult> Create(int? toolTypeId)
        {
            // Check if the tool type ID is null
            if (toolTypeId == null)
            {
                // If tool type ID is null, return a "NotFound" result
                return NotFound();
            }

            // Retrieve the tool type with the specified ID from the database
            List<ToolType> allTools = await _context.ToolTypes.ToListAsync();
            // Retrieve the tool type with the specified ID
            var toolType = allTools.Find(x => x.Id == toolTypeId);
            // Check if the tool type with the specified ID is not found
            if (toolType == null)
            {
                // If tool type is not found, return a "NotFound" result
                return NotFound();
            }

            // Set ViewBag properties for the view
            ViewBag.ToolTypeImage = toolType.Image;
            ViewBag.ToolType = toolType;

            // Return the view for creating a new rental with details related to the specified tool type
            return View();
        }


        /// <summary>
        /// Handles the HTTP POST request for creating a new rental.
        /// </summary>
        /// <param name="rentInput">The rental information to be created.</param>
        /// <returns>A redirect to the 'RentHistory' action if the creation is successful; otherwise, a validation error.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("StartOfRent,EndOfRent,DiscountId")] Rent rentInput, int? coupon)
        {
            // Check if the return date is earlier than the start date
            if (rentInput.EndOfRent < rentInput.StartOfRent)
            {
                return BadRequest("Date of return is earlier than date of taking");
            }

            // Check if the start date or return date is earlier than today
            if (rentInput.StartOfRent < DateTime.Now || rentInput.EndOfRent < DateTime.Now)
            {
                return BadRequest("Date of taking or date of return is earlier than today");
            }

            // Set the user ID from the claims associated with the current user
            rentInput.UserId = Int32.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            // Retrieve the tool type ID from the session
            var toolTypeId = _sessionService.GetInt32("ToolType");

            // Query the database to find available tools based on the tool type and date range
            var tools = await _context.Tools
                .Where(tool => tool.ToolTypeId == toolTypeId &&
                               !_context.Rents.Any(rent =>
                                   rent.ToolId == tool.Id &&
                                   (rentInput.StartOfRent < rent.EndOfRent && rentInput.EndOfRent > rent.StartOfRent)))
                .ToListAsync();



            // Check if available tools were found
            if (tools.Count > 0)
            {
                // Set the tool ID to the first available tool
                rentInput.ToolId = tools[0].Id;
            }
            else
            {
                // Return a BadRequest response if no tools are available in the specified period
                return BadRequest("There are no tools available in this period");
            }
            
            // Retrieve the tool type from the database
            var toolType = await _context.ToolTypes.FindAsync(toolTypeId);

            // Calculate the rent price based on the tool type's price and the rental duration
            rentInput.RentPrice = toolType.Price * rentInput.EndOfRent.Subtract(rentInput.StartOfRent).TotalDays;
            if (coupon != null)
            {
                try
                {
                    var discounts = await _context.Discounts.ToListAsync();
                    tools[0].ToolType = toolType;
                    rentInput.RentPrice = CalculateDiscount(tools[0], coupon??=-1, discounts) * rentInput.EndOfRent.Subtract(rentInput.StartOfRent).TotalDays;
                    rentInput.DiscountId = coupon;
                }
                catch (Exception ex)
                {
                    rentInput.DiscountId = null;
                }
            }
                

            // Check if the model state is valid
            if (ModelState.IsValid)
            {
                // Add the rent to the context and save changes
                _context.Add(rentInput);
                await _context.SaveChangesAsync();

                // Redirect to the RentHistory action if successful
                return RedirectToAction("RentHistory");
            }

            // Redirect to the Create action with the tool type ID if the model state is not valid
            return RedirectToAction("Create", new { toolTypeId });
        }
    }
}


using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Burgija.Models
{
    public class Location
    {
        #region Properties

        [Key]
        public int Id { get; set; }
        [Required]
        public double XCoordinate { get; set; }
        [Required]
        public double YCoordinate { get; set; }
        [Required]
        public string Address { get; set; }

        #endregion

        #region Constructors

        public Location(int id, double xCoordinate, double yCoordinate, string address)
        {
            if (id <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(id), "Id mora biti ve�i od 0.");
            }
            Id = id;
            XCoordinate = xCoordinate;
            YCoordinate = yCoordinate;
            Address = address;
        }

        public Location() { }

        #endregion

        #region Methods

        //TODO

        #endregion
    }
}
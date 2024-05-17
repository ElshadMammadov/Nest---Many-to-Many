using System.ComponentModel.DataAnnotations;

namespace PustokImagesUploadTask.ViewModels
{
    public class OrderViewModel
    {
        public List<CheckOutViewModel>? CheckOutViewModels { get; set; }

        [StringLength(maximumLength: 50)]
        public string FullName { get; set; }
        [StringLength(maximumLength: 50)]

        public string Country { get; set; }
        [StringLength(maximumLength: 50)]
        public string City { get; set; }
        [StringLength(maximumLength: 89),DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [StringLength(maximumLength: 60),DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }
        [StringLength(maximumLength: 150)]
        public string Address { get; set; }

        [StringLength(maximumLength: 30)]
        public string ZipCode { get; set; }

        [StringLength(maximumLength: 1500)]
        public string? Note { get; set; }
    }
}

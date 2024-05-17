using NuGet.Protocol.Core.Types;
using PustokImagesUploadTask.Base;

namespace PustokImagesUploadTask.Models
{
    public class BasketItem : BaseEntity
    {
        public string AppUserId { get; set; }
        public int BookId{ get; set; }
        public int Count { get; set; }
        public bool IsDeleted { get; set; }
        public Book? Book { get; set; }
        public AppUser? AppUser { get; set; }
    }
}

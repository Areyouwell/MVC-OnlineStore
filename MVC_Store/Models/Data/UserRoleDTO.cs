using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVC_Store.Models.Data
{
    [Table("tblUserRoles")]
    public class UserRoleDTO
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }

        [ForeignKey("UserId")]
        public virtual UserDTO User { get; set; }
        [ForeignKey("RoleId")]
        public virtual RoleDTO Role { get; set; }
    }
}

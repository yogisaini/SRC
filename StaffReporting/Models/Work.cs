using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Management.Models
{
    public partial class Work
    {
        public int Id { get; set; }
        [Required]
        [Display(Name = "Project Name")]
        public string? Title { get; set; }
        [Required]
        public string? Description { get; set; }
        public string? Priority { get; set; }
        public string? Code { get; set; }
        [Required]
        [Display(Name = "Assign a Manager")]
        public int UserId { get; set; }

        [Required]
        [Display(Name = "Department")]
        public int DeptId { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? UpdateBy { get; set; }
        public DateTime UpdateDate { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDelete { get; set; } = false;
        public bool IsComplete { get; set; } = false;
        public DateTime? DeadLine { get; set; }

        public List<WriteUp> WriteUps { get; set; } = new List<WriteUp>();

        [ForeignKey("UserId")]

        public virtual Users? Users { get; set; }

        [ForeignKey("DeptId")]
        public virtual Dept? Dept { get; set; }
    }
    public partial class WriteUp
    {
        public int Id { get; set; }
        [Required]
        [Display(Name = "Project Name")]
        public int WorkId { get; set; }
        public string? CreatedBy { get; set; }
        public int UserId { get; set; }
        [Required]
        [Display(Name = "Remark")]
        public string? Remark { get; set; }
        [Required]
        [Display(Name = "Task Name")]
        public int Task { get; set; }
        public DateTime SubmittedDate { get; set; }
        public bool IsActive { get; set; } = true;
        [Required]
        public List<Linkpaths> Linkpaths { get; set; } = new List<Linkpaths>();

        public List<Comment> Comment { get; set; } = new List<Comment>();

        [ForeignKey("UserId")]
        public virtual Users Users { get; set; }

        [ForeignKey("WorkId")]
        public virtual Work Work { get; set; }

        [ForeignKey("Task")]
        public virtual Tasklist? Tasklist { get; set; }
    }
    public partial class Users
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Full Name")]
        public string Username { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "Password")]
        public string PasswordHash { get; set; }

        [Required]
        public int DesiId { get; set; }

        [Required]
        [StringLength(20)]
        public string Role { get; set; } = "User";

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        [StringLength(100)]
        public string? Mobile { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdateDate { get; set; } = DateTime.UtcNow;
        [Required]
        public bool IsActive { get; set; } = true;
        public bool IsDelete { get; set; } = false;

        public bool permission { get; set; } = false;

        [ForeignKey("DesiId")]
        public virtual Desi? Desi { get; set; }
    }

    public partial class Linkpaths
    {
        [Key]
        public int AttachmentId { get; set; }
        public int WriteUpId { get; set; }
        public string? Link { get; set; }
    }
    public partial class Desi
    {
        [Key]
        public int DesiId { get; set; }
        [Display(Name = "Designation Name")]
        public string? DesiName { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public bool IsDelete { get; set; } = false;
    }
    public partial class Dept
    {
        [Key]
        public int DeptId { get; set; }
        [Display(Name = "Department Name")]
        public string? DeptName { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public bool IsDelete { get; set; } = false;
    }
    public partial class Comment
    {
        [Key]
        public int CommentID { get; set; }
        public string? CommentText { get; set; }
        public DateTime CommentDate { get; set; } = DateTime.UtcNow;
        public int UserId { get; set; }
        public int WriteUpId { get; set; }
        public bool CommentRead { get; set; }

        [ForeignKey("UserId")]
        public virtual Users? Users { get; set; }
    }
    public partial class Tasklist
    {
        [Key]
        public int TaskID { get; set; }
        public string? TaskName { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public bool IsDelete { get; set; } = false;
    }
    public partial class WorkAssignment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Work")]
        public int WorkId { get; set; }
        public virtual Work? Work { get; set; }

        [Required]
        [ForeignKey("Users")]
        public int UserId { get; set; }
        public virtual Users User { get; set; }
    }
    public partial class ProjectAssignUser
    {
        public int UserID { get; set; }
        public string? UserName { get; set; }
        public string? Designation { get; set; }
    }        
}

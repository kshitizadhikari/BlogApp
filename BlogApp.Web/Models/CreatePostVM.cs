﻿using BlogApp.Web.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace BlogApp.Web.Models
{
    public class CreatePostVM
    {
        [Required]
        public string Title { get; set; }
        [Required]
        public string Content { get; set; }

    }
}

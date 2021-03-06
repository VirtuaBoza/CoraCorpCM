﻿using System.ComponentModel.DataAnnotations;

namespace CoraCorpCM.Application.Models
{
    public class Inspector : IEntity<int>
    {
        public int Id { get; set; }

        public Museum Museum { get; set; }
        public int MuseumId { get; set; }

        [Required]
        public string Name { get; set; }
    }
}

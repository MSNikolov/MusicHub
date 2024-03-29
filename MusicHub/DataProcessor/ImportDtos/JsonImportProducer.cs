﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MusicHub.DataProcessor.ImportDtos
{
    public class JsonImportProducer
    {
        [Required]
        [MinLength(3), MaxLength(30)]
        public string Name { get; set; }

        [RegularExpression(@"^[A-Z][a-z]+ [A-Z][a-z]+$")]
        public string Pseudonym { get; set; }


        [RegularExpression(@"^[+]359 \d{3} \d{3} \d{3}$")]
        public string PhoneNumber { get; set; }

        public HashSet<JsonImportAlbum> Albums { get; set; } = new HashSet<JsonImportAlbum>();
    }
}

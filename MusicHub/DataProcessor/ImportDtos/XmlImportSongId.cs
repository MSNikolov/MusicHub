﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace MusicHub.DataProcessor.ImportDtos
{
    [XmlType("Song")]
    public class XmlImportSongId
    {
        [XmlAttribute("id")]
        public int Id { get; set; }
    }
}

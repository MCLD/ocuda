﻿namespace Ocuda.Ops.Models.Entities
{
    public class SectionFileLibrary
    {
        public int SectionId { get; set; }

        public Section Section { get; set; }

        public int FileLibraryId { get; set; }

        public FileLibrary FileLibrary { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Text;

namespace IMS_Application.DTOs
{
    public class AttentionItemDto
    {
        public string Label { get; set; } = string.Empty;

        public int Count { get; set; }

        public string Severity { get; set; } = string.Empty;
    }
}


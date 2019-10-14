using System;
using System.Collections.Generic;

namespace EFCore.SkipBug
{
    public class RootEntity
    {
        public Guid Id { get; set; }
        public List<SubEntity> SubEntities { get; set; } = new List<SubEntity>();
    }

    public class SubEntity
    {
        public RootEntity Root { get; set; }
        public Guid RootId { get; set; }
        public Guid CollectMe { get; set; }
    }
}

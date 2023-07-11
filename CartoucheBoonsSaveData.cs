using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using EgyptianCartouches.Boons;
using EgyptianCartouches.Items;
using ThunderRoad;

namespace EgyptianCartouches
{
    [Serializable]
    public class CartoucheBoonsSaveData
    {
        public string cartoucheId { get; set; }
        public List<string> itemsId { get; set; }
    }
}

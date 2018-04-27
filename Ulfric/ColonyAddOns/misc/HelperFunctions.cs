using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pipliz;
using Pipliz.JSON;

namespace Ulfric.ColonyAddOns
{
    class HelperFunctions
    {
        public JSONNode ConvertItemToJson(JSONNode root, ItemTypes.ItemType item)
        {

            //JSONNode itemnode = new JSONNode(NodeType.Object);
            JSONNode itemnodevalue = new JSONNode();

            itemnodevalue.SetAs<bool>("blockPathing", item.BlocksPathing);

            JSONNode node = new JSONNode();
            if (item.BoxColliders != null)
            {
                JSONNode collidersnode = new JSONNode(NodeType.Array);
                foreach (BoundsPip bp in item.BoxColliders)
                {
                    JSONNode subNode = new JSONNode();
                    JSONNode paramNode = new JSONNode(NodeType.Array);
                    JSONNode param = new JSONNode();
                    param.SetAs<float>(bp.Max.x);
                    paramNode.AddToArray(param);
                    param.SetAs<float>(bp.Max.y);
                    paramNode.AddToArray(param);
                    param.SetAs<float>(bp.Max.z);
                    paramNode.AddToArray(param);
                    subNode.SetAs("max", paramNode);

                    paramNode = new JSONNode(NodeType.Array);
                    param.SetAs<float>(bp.Min.x);
                    paramNode.AddToArray(param);
                    param.SetAs<float>(bp.Min.y);
                    paramNode.AddToArray(param);
                    param.SetAs<float>(bp.Min.z);
                    paramNode.AddToArray(param);
                    subNode.SetAs("min", paramNode);
                    collidersnode.AddToArray(subNode);
                }
                node.SetAs("boxes", collidersnode);
                node.SetAs<bool>("collidePlayer", item.CollidePlayer);
                node.SetAs<bool>("collideSelection", item.CollideSelection);
                itemnodevalue.SetAs("colliders", node);
            }

            if (item.Categories != null)
            {
                node = new JSONNode(NodeType.Array);
                foreach (string c in item.Categories)
                {
                    node.SetAs<string>(c);
                }
                itemnodevalue.SetAs("catagories", node);
            }

            itemnodevalue.SetAs("color", item.Color.Linear());

            if (item.CustomDataNode != null)
            {
                node = new JSONNode();
                foreach(KeyValuePair<string, JSONNode> cdn in item.CustomDataNode.LoopObject())
                {
                    JSONNode subNode = new JSONNode();
                    node.SetAs(cdn.Key, cdn.Value);
                }
                itemnodevalue.SetAs("customData", node);
            }

            itemnodevalue.SetAs<int>("destructionTime", item.DestructionTime);
            itemnodevalue.SetAs<string>("icon", item.Icon == null ? "" : item.Icon);
            itemnodevalue.SetAs<bool>("isDestructible", item.IsDestructible);
            itemnodevalue.SetAs<bool>("isFertile", item.IsFertile);
            itemnodevalue.SetAs<bool>("isPlaceable", item.IsPlaceable);
            itemnodevalue.SetAs<bool>("isRotatable", item.IsRotatable);
            itemnodevalue.SetAs<bool>("isSolid", item.IsSolid);
            itemnodevalue.SetAs<ushort>("itemIndex", item.ItemIndex);
            itemnodevalue.SetAs<ushort>("maxStackSize", item.MaxStackSize);
            itemnodevalue.SetAs<string>("mesh", item.Mesh == null ? "" : item.Mesh);
            itemnodevalue.SetAs<bool>("needBase", item.NeedsBase);
            itemnodevalue.SetAs<float>("nutritionalValue", item.NutritionalValue);
            itemnodevalue.SetAs<string>("onPlaceAudio", item.OnPlaceAudio == null ? "" : item.OnPlaceAudio);
            itemnodevalue.SetAs<string>("onRemoveAudio", item.OnRemoveAudio == null ? "" : item.OnRemoveAudio);

            if (item.OnRemoveItems != null)
            {
                node = new JSONNode(NodeType.Array);
                foreach(ItemTypes.ItemTypeDrops itd in item.OnRemoveItems)
                {
                    JSONNode subNode = new JSONNode();
                    subNode.SetAs<double>(ItemTypes.IndexLookup.GetName(itd.item.Type), itd.chance);
                    node.AddToArray(subNode);
                }
                itemnodevalue.SetAs("onRemoveItems", node);
            }

            itemnodevalue.SetAs<string>("parentType", item.ParentType == null ? "" : item.ParentType);
            itemnodevalue.SetAs<string>("rotatedX-", item.RotatedXMinus == null ? "" : item.RotatedXMinus);
            itemnodevalue.SetAs<string>("rotatedX+", item.RotatedXMinus == null ? "" : item.RotatedXPlus);
            itemnodevalue.SetAs<string>("rotatedZ-", item.RotatedXMinus == null ? "" : item.RotatedZMinus);
            itemnodevalue.SetAs<string>("rotatedZ+", item.RotatedXMinus == null ? "" : item.RotatedZPlus);
            itemnodevalue.SetAs<string>("sideAll", item.SideAll == null ? "" : item.SideAll);
            itemnodevalue.SetAs<string>("sidex-", item.SideXMinus == null ? "" : item.SideXMinus);
            itemnodevalue.SetAs<string>("sidex+", item.SideXPlus == null ? "" : item.SideXPlus);
            itemnodevalue.SetAs<string>("sidey-", item.SideYMinus== null ? "" : item.SideYMinus);
            itemnodevalue.SetAs<string>("sidey+", item.SideYPlus == null ? "" : item.SideYPlus);
            itemnodevalue.SetAs<string>("sidez-", item.SideZMinus == null ? "" : item.SideZMinus);
            itemnodevalue.SetAs<string>("sidez+", item.SideZPlus == null ? "" : item.SideZPlus);

            root.SetAs(item.Name, itemnodevalue);


            return root;
        }

        public void DumpItemsToJSON(string filenameandpath, List<ushort> itemlist)
        {
            HelperFunctions hf = new HelperFunctions();
            JSONNode root = new JSONNode();
            foreach(ushort i in itemlist)
                root = hf.ConvertItemToJson(root, ItemTypes.GetType(i));
            JSON.Serialize(filenameandpath, root);
        }

    }
}

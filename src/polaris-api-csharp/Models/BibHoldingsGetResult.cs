using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Clc.Polaris.Api.Models
{

    /// <summary>
    /// BibHoldingsGet result
    /// </summary>
    [XmlRoot(ElementName = "BibHoldingsGetResult")]
    public class BibHoldingsGetResult : PapiResponseCommon
    {
        /// <summary>
        /// Container for bib holding rows
        /// </summary>
        [XmlElement(ElementName = "BibHoldingsGetRows")]
        public BibHoldingsGetRows BibHoldingsGetRows { get; set; }
    }

    /// <summary>
    /// Big holding rows
    /// </summary>
    [XmlRoot(ElementName = "BibHoldingsGetRows")]
    public class BibHoldingsGetRows
    {
        /// <summary>
        /// List of bib holding data
        /// </summary>
        [XmlElement(ElementName = "BibHoldingsGetRow")]
        public List<BibHoldingsGetRow> BibHoldingsGetRow { get; set; }
    }

    /// <summary>
    /// A row of bib holdings data
    /// </summary>
    [XmlRoot(ElementName = "BibHoldingsGetRow")]
    public class BibHoldingsGetRow
    {
        /// <summary>
        /// Item's assigned branch ID
        /// </summary>
        [XmlElement(ElementName = "LocationID")]
        public string LocationID { get; set; }

        /// <summary>
        /// Item's assigned branch name
        /// </summary>
        [XmlElement(ElementName = "LocationName")]
        public string LocationName { get; set; }

        /// <summary>
        /// CollectionID
        /// </summary>
        [XmlElement(ElementName = "CollectionID")]
        public string CollectionID { get; set; }

        /// <summary>
        /// Collection name
        /// </summary>
        [XmlElement(ElementName = "CollectionName")]
        public string CollectionName { get; set; }

        /// <summary>
        /// Barcode
        /// </summary>
        [XmlElement(ElementName = "Barcode")]
        public string Barcode { get; set; }

        /// <summary>
        /// Public note
        /// </summary>
        [XmlElement(ElementName = "PublicNote")]
        public string PublicNote { get; set; }

        /// <summary>
        /// Call number
        /// </summary>
        [XmlElement(ElementName = "CallNumber")]
        public string CallNumber { get; set; }

        /// <summary>
        /// Designation
        /// </summary>
        [XmlElement(ElementName = "Designation")]
        public string Designation { get; set; }

        /// <summary>
        /// Shelf location
        /// </summary>
        [XmlElement(ElementName = "ShelfLocation")]
        public string ShelfLocation { get; set; }

        /// <summary>
        /// Circulation status
        /// </summary>
        [XmlElement(ElementName = "CircStatus")]
        public string CircStatus { get; set; }

        /// <summary>
        /// Last circulation date
        /// </summary>
        [XmlElement(ElementName = "LastCircDate")]
        public string LastCircDate { get; set; }

        /// <summary>
        /// Material type
        /// </summary>
        [XmlElement(ElementName = "MaterialType")]
        public string MaterialType { get; set; }

        /// <summary>
        /// Textual holdings note
        /// </summary>
        [XmlElement(ElementName = "TextualHoldingsNote")]
        public string TextualHoldingsNote { get; set; }

        /// <summary>
        /// Retention statement
        /// </summary>
        [XmlElement(ElementName = "RetentionStatement")]
        public string RetentionStatement { get; set; }

        /// <summary>
        /// Holdings statement
        /// </summary>
        [XmlElement(ElementName = "HoldingsStatement")]
        public string HoldingsStatement { get; set; }

        /// <summary>
        /// Holdings note
        /// </summary>
        [XmlElement(ElementName = "HoldingsNote")]
        public string HoldingsNote { get; set; }

        /// <summary>
        /// Total items at the assigned location
        /// </summary>
        [XmlElement(ElementName = "ItemsTotal")]
        public string ItemsTotal { get; set; }

        /// <summary>
        /// Items available for checkouts
        /// </summary>
        [XmlElement(ElementName = "ItemsIn")]
        public string ItemsIn { get; set; }

        /// <summary>
        /// Holdable
        /// </summary>
        [XmlElement(ElementName = "Holdable")]
        public string Holdable { get; set; }

        /// <summary>
        /// Volume number
        /// </summary>
        [XmlElement(ElementName = "VolumeNumber")]
        public string VolumeNumber { get; set; }

        /// <summary>
        /// Due date
        /// </summary>
        [XmlElement(ElementName = "DueDate")]
        public string DueDate { get; set; }

        /// <summary>
        /// Is this material type chargeable
        /// </summary>
        [XmlElement(ElementName = "IsMaterialTypeChargeable")]
        public string IsMaterialTypeChargeable { get; set; }

        /// <summary>
        /// Is this an electronic item
        /// </summary>
        [XmlElement(ElementName = "IsElectronicItem")]
        public string IsElectronicItem { get; set; }
    }
}
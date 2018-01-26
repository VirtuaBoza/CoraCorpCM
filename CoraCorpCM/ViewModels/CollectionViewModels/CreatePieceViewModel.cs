﻿using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CoraCorpCM.ViewModels.CollectionViewModels
{
    public class CreatePieceViewModel
    {
        // Piece Info
        [Display(Name = "Assession Number")]
        public string AccessionNumber { get; set; }

        [Required]
        public string Title { get; set; }

        public string Subject { get; set; }

        [Display(Name = "Medium")]
        public string MediumId { get; set; }
        public IEnumerable<SelectListItem> Media { get; set; }

        [Display(Name = "New Medium")]
        public string MediumName { get; set; }

        [Display(Name = "Genre")]
        public string GenreId { get; set; }
        public IEnumerable<SelectListItem> Genres { get; set; }

        // Genre - New
        [Display(Name = "New Genre")]
        public string GenreName { get; set; }

        // Subgenre - Existing
        [Display(Name = "Subgenre")]
        public string SubgenreId { get; set; }
        public IEnumerable<SelectListItem> Subgenres { get; set; }

        [Display(Name = "New Subgenre")]
        public string SubgenreName { get; set; }

        [Display(Name = "Subject Matter")]
        public string SubjectMatterId { get; set; }
        public IEnumerable<SelectListItem> SubjectMatters { get; set; }

        [Display(Name = "New Subject Matter")]
        public string SubjectMatterName { get; set; }

        [Display(Name = "Estimated Value")]
        [DataType(DataType.Currency)]
        public decimal? EstimatedValue { get; set; }

        [Display(Name = "Framed")]
        public bool IsFramed { get; set; }

        // Artist Info
        [Display(Name = "Artist")]
        public string ArtistId { get; set; }
        public IEnumerable<SelectListItem> KnownArtists { get; set; }

        #region Artist - New
        [Display(Name = "Name")]
        public string ArtistName { get; set; }

        [Display(Name = "Also Known As")]
        public string ArtistAlsoKnownAs { get; set; }

        [Display(Name = "City")]
        public string ArtistCity { get; set; }

        [Display(Name = "State")]
        public string ArtistState { get; set; }

        [Display(Name = "Country")]
        public string ArtistCountryId { get; set; }

        [Display(Name = "Birth Date")]
        public string ArtistBirthdate { get; set; }

        [Display(Name = "Death Date")]
        public string ArtistDeathdate { get; set; }
        #endregion

        // Creation Date
        [Display(Name = "Year")]
        public string CreationYear { get; set; }

        [Display(Name = "Month")]
        [Range(1, 12)]
        public string CreationMonth { get; set; }

        [Display(Name = "Day")]
        [Range(1,31)]
        public string CreationDay { get; set; }

        // Origin
        [Display(Name = "City")]
        public string OriginCity { get; set; }

        [Display(Name = "State")]
        public string OriginState { get; set; }

        [Display(Name = "Country")]
        public string OriginCountryId { get; set; }
        public IEnumerable<SelectListItem> Countries { get; set; }

        // Dimensions
        [Range(0,double.PositiveInfinity)]
        public double? Height { get; set; }

        [Range(0, double.PositiveInfinity)]
        public double? Width { get; set; }

        [Range(0, double.PositiveInfinity)]
        public double? Depth { get; set; }

        [Display(Name = "Units")]
        public string UnitOfMeasureId { get; set; }
        public IEnumerable<SelectListItem> UnitsOfMeasure { get; set; }

        [Display(Name = "Year")]
        public string CopyrightYear { get; set; }

        [Display(Name = "Owner")]
        public string CopyrightOwner { get; set; }

        // Insurance Info
        [Display(Name = "Policy Number")]
        public string PolicyNumber { get; set; }

        [Display(Name = "Amount Insured")]
        public decimal? AmountInsured { get; set; }

        [Display(Name = "Expiration Date")]
        public string ExpirationDate { get; set; }

        public string Carrier { get; set; }

        // Acquisition - Existing
        public string AcquisitionId { get; set; }
        public IEnumerable<SelectListItem> Acquisitions { get; set; }

        #region Acquisition - New
        [Display(Name = "Acquisition Date")]
        public string AcquisitionDate { get; set; }

        [Display(Name = "Source")]
        public string PieceSourceId { get; set; }
        public IEnumerable<SelectListItem> PieceSources { get; set; }

        [Display(Name = "New Source")]
        public string PieceSourceName { get; set; }

        [DataType(DataType.Currency)]
        public decimal? Cost { get; set; }

        [Display(Name = "Funding Source")]
        public string FundingSourceId { get; set; }
        public IEnumerable<SelectListItem> FundingSources { get; set; }

        [Display(Name = "New Funding Source")]
        public string FundingSourceName { get; set; }

        public string Terms { get; set; }
        #endregion
    }
}
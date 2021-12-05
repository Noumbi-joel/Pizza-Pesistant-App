using PizzaApp.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace PizzaApp.Models
{
    class Pizza
    {
        //nom, prix, ingredients
        public string nom { get; set; }
        public int prix { get; set; }
        public string[] ingredients { get; set; }

        public string imageUrl { get; set; }

        public string PrixEuros { get { return prix + "£"; } }

        public string IngredientStr { get { return String.Join(", ", ingredients);} }

        public string Titre { get { return StringExtensions.PremiereLettreMajuscule(nom); } }

        public Pizza()
        {

        }
    }
}

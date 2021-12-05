using Newtonsoft.Json;
using PizzaApp.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace PizzaApp
{
    public partial class MainPage : ContentPage
    {
        List<Pizza> pizzas;

        List<string> pizzasFav = new List<string>();
        enum e_tri
        {
            TRI_AUCUN,
            TRI_PRIX,
            TRI_NOM,
            TRI_FAV
        }

        e_tri tri = e_tri.TRI_AUCUN;

        const string KEY_TRI = "tri";
        const string KEY_FAV = "favorite";

        string tempFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "temp");
        string jsonFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "pizzas.json");


        public MainPage()
        {
            InitializeComponent();

            /*
            pizzasFav.Add("4 fromages");
            pizzasFav.Add("tartiflete");
            pizzasFav.Add("indienne");
            */
            LoadFavList();

            if (Application.Current.Properties.ContainsKey(KEY_TRI))
            {
                tri = (e_tri)Application.Current.Properties[KEY_TRI];
                imgButton.Source = GetImageSourceFromTri(tri);
            }


            maListePizza.RefreshCommand = new Command((obj) =>
            {
                Console.WriteLine("Refresh Command");
                downloadData((pizzas) =>
                {
                    if (pizzas != null)
                    {
                        maListePizza.ItemsSource = GetPizzaCells(GetPizzasFromTri(tri, pizzas), pizzasFav);
                    }
                    maListePizza.IsRefreshing = false;
                });

            });

            downloadData((pizzas) => 
            {
                if(pizzas != null)
                {
                    maListePizza.ItemsSource = GetPizzaCells(GetPizzasFromTri(tri, pizzas), pizzasFav);
                }
                activityIndic.IsRunning = false;
                maListePizza.IsVisible = true;
            });

            if (File.Exists(jsonFileName))
            {
                string pizzasJson = File.ReadAllText(jsonFileName);
                if (!String.IsNullOrEmpty(pizzasJson))
                {
                    pizzas = JsonConvert.DeserializeObject<List<Pizza>>(pizzasJson);
                    maListePizza.ItemsSource = GetPizzaCells(GetPizzasFromTri(tri, pizzas), pizzasFav);
                    activityIndic.IsRunning = false;
                    maListePizza.IsVisible = true;
                }
            }
        }

        private void downloadData(Action<List<Pizza>> action)
        {
            const string url = "https://drive.google.com/uc?export=download&id=1RvBrdPr6a024UGb9HJtxYpekSkIB9Jut";

            using (var webClient = new WebClient())
            {
                    webClient.DownloadFileCompleted += (object sender, AsyncCompletedEventArgs e) =>
                    {
                        Exception ex = e.Error;

                        if(ex == null)
                        {
                            File.Copy(tempFileName, jsonFileName, true);

                            string pizzasJson = File.ReadAllText(jsonFileName);

                            pizzas = JsonConvert.DeserializeObject<List<Pizza>>(pizzasJson);

                            Device.BeginInvokeOnMainThread(() =>
                            {
                                action.Invoke(pizzas);
                            });
                        }
                        else
                        {
                            Device.BeginInvokeOnMainThread(async () =>
                            {
                                await DisplayAlert("ERREUR", "Une erreur réseau s'est produite: " + ex.Message, "OK");
                                action.Invoke(null);
                            });
                        }
                    };
                    webClient.DownloadFileAsync(new Uri(url), tempFileName);
                }
            }

        private string GetImageSourceFromTri(e_tri t)
        {
            if (tri.Equals(e_tri.TRI_AUCUN))
            {
                return "sort_none.png";
            }

            else if (tri.Equals(e_tri.TRI_NOM))
            {
                return "sort_nom.png";
            }

            else if (tri.Equals(e_tri.TRI_PRIX))
            {
                return "sort_prix.png";
            }

            else if (tri.Equals(e_tri.TRI_FAV))
            {
                return "sort_fav.png";
            }

            return "sort_none.png";
        }

        private void handleClicked(object sender, EventArgs e)
        {
            if (tri.Equals(e_tri.TRI_AUCUN))
            {
                tri = e_tri.TRI_NOM;
            }

            else if (tri.Equals(e_tri.TRI_NOM))
            {
                tri = e_tri.TRI_PRIX;
            }

            else if (tri.Equals(e_tri.TRI_PRIX))
            {
                tri = e_tri.TRI_FAV;
            }

            else if (tri.Equals(e_tri.TRI_FAV)){
                tri = e_tri.TRI_AUCUN;
            }

            imgButton.Source = GetImageSourceFromTri(tri);
            maListePizza.ItemsSource = GetPizzaCells(GetPizzasFromTri(tri, pizzas), pizzasFav);

            Application.Current.Properties[KEY_TRI] = (int)tri;
            Application.Current.SavePropertiesAsync();

        }

        private List<Pizza> GetPizzasFromTri(e_tri t, List<Pizza> l)
        {
            if(l == null)
            {
                return null;
            }

            if (tri.Equals(e_tri.TRI_NOM) || tri.Equals(e_tri.TRI_FAV))
            {
                List<Pizza> ret = new List<Pizza>(l);
                ret.Sort((p1, p2) => { return p1.Titre.CompareTo(p2.Titre); });
                return ret;
            }

            else if (tri.Equals(e_tri.TRI_PRIX) )
            {
                List<Pizza> ret = new List<Pizza>(l);
                ret.Sort((p1, p2) => { return p2.prix.CompareTo(p1.prix); });
                return ret;
            }

            return l;
        }

        private void OnFavPizzaChanged(PizzaCell pizzaCell)
        {
            bool isInFavList = pizzasFav.Contains(pizzaCell.pizza.nom);

            if(pizzaCell.isFavorite && !isInFavList)
            {
                pizzasFav.Add(pizzaCell.pizza.nom);
            }
            else if (!pizzaCell.isFavorite && isInFavList){
                pizzasFav.Remove(pizzaCell.pizza.nom);
            }
            SaveFavList();
        }

        private List<PizzaCell> GetPizzaCells(List<Pizza> p, List<string> f)
        {
            List<PizzaCell> ret = new List<PizzaCell>();
            List<PizzaCell> temp = new List<PizzaCell>(); ;


            if (p == null)
            {
                return ret;
            }

            foreach(Pizza pizza in p)
            {
                bool isFav = f.Contains(pizza.nom);

                if (tri.Equals(e_tri.TRI_FAV))
                {
                    if (isFav)
                    {
                        ret.Add(new PizzaCell { pizza = pizza, isFavorite = isFav, favChangedAction = OnFavPizzaChanged });
                    }
                }

                else
                {
                    ret.Add(new PizzaCell { pizza = pizza, isFavorite = isFav, favChangedAction = OnFavPizzaChanged });
                }
            }

            return ret;
        }

        private void SaveFavList()
        {
            string json = JsonConvert.SerializeObject(pizzasFav);
            Application.Current.Properties[KEY_FAV] = json;
            Application.Current.SavePropertiesAsync();
        }

        private void LoadFavList()
        {
            if (Application.Current.Properties.ContainsKey(KEY_FAV))
            {
                string json = Application.Current.Properties[KEY_FAV].ToString();
                pizzasFav = JsonConvert.DeserializeObject<List<string>>(json);
            }
        }
    }
}

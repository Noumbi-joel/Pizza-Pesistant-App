using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace PizzaApp.Models
{
    class PizzaCell : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public Pizza pizza { get; set; }
        public bool isFavorite { get; set; }
        public string imageSourceFav { get { return isFavorite ? "star2.png" : "star1.png"; } }

        public ICommand FavClickCommand { get; set; }

        public Action<PizzaCell> favChangedAction { get; set; }

        public PizzaCell()
        {
            FavClickCommand = new Command((obj) =>
            {
                Console.WriteLine("FavClickCommand");
                isFavorite = !isFavorite;
                OnPropertyChanged("imageSourceFav");
                favChangedAction.Invoke(this);
            });
        }

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}

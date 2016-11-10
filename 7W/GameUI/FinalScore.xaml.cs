using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SevenWonders
{
    /// <summary>
    /// Interaction logic for FinalScore.xaml
    /// </summary>
    public partial class FinalScore : Window
    {
        MainWindow mainWindow;

        public FinalScore(MainWindow mw, NameValueCollection scores)
        {
            InitializeComponent();

            mainWindow = mw;

            FontFamily ourFont = new FontFamily("Lucida Handwriting");

            Width = 78 + (scores.Count) * 62;

            int column = 1;
            foreach (string s in scores.AllKeys)
            {
                ScoreGrid.ColumnDefinitions.Add(new ColumnDefinition());

                TextBlock nameElement = new TextBlock()
                {
                    Text = s,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    FontFamily = ourFont,
                    FontSize = 10,
                };

                Grid.SetRow(nameElement, 0);
                Grid.SetColumn(nameElement, column);
                ScoreGrid.Children.Add(nameElement);

                string[] st = scores[s].Split(',');

                int row = 1;
                foreach (string str in scores[s].Split(','))
                {
                    TextBlock v = new TextBlock()
                    {
                        Text = str,
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        FontFamily = ourFont,
                        FontSize = 24,
                    };

                    Grid.SetRow(v, row);
                    Grid.SetColumn(v, column);

                    ScoreGrid.Children.Add(v);

                    ++row;
                }

                ++column;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            mainWindow.Close();
        }
    }
}

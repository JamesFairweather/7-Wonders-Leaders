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
    /// Interaction logic for GetDebtToken.xaml
    /// </summary>
    public partial class GetDebtToken : Window
    {
        Coordinator coordinator;

        int coinsInTreasury;
        int coinsToLose;

        public GetDebtToken(Coordinator coordinator, NameValueCollection p)
        {
            InitializeComponent();

            this.coordinator = coordinator;

            coinsInTreasury = int.Parse(p["coin"]);
            coinsToLose = int.Parse(p["coinsToLose"]);

            lblMessageToPlayer.Content = string.Format("You have a {0}-coin debt to pay.\n", coinsToLose);
            lblMessageToPlayer.Content += string.Format("There are currently {0} coins in your treasury.\n", coinsInTreasury);
            lblMessageToPlayer.Content += string.Format("How much of this debt to you want to pay now?\n");
            lblMessageToPlayer.Content += string.Format("Any debt not paid off immediately counts as minus 1\n");
            lblMessageToPlayer.Content += string.Format("Victory Points at the conclusion of the game.\n");

            sliderResponse.Maximum = coinsToLose;
            sliderResponse.Minimum = Math.Max(0, coinsToLose - coinsInTreasury);

            // The default is for the player to pay all of the debt (or as much as possible)
            sliderResponse.Value = sliderResponse.Minimum;

            lblDebt.Content = sliderResponse.Value;
            lblCoins.Content = coinsInTreasury - (coinsToLose - sliderResponse.Value);
        }

        private void sliderResponse_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            lblCoins.Content = coinsInTreasury - (coinsToLose -  sliderResponse.Value);
            lblDebt.Content = sliderResponse.Value;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            coordinator.sendToHost(string.Format("DebtResponse&DebtTokens={0}", sliderResponse.Value));
            Close();
        }
    }
}

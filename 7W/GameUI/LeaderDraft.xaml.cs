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
    /// Interaction logic for LeaderDraft.xaml
    /// </summary>
    public partial class LeaderDraft : Window
    {
        Coordinator coordinator;
        bool isCourtesanSelection;

        public LeaderDraft(Coordinator c, bool courtesanSelection)
        {
            InitializeComponent();

            //make graphics better
            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.Fant);

            coordinator = c;

            isCourtesanSelection = courtesanSelection;

            if (isCourtesanSelection)
                Instructions.Text = "Choose a neighbor's leader to copy.";
        }

        public void UpdateUI(NameValueCollection cards)
        {
            hand.Items.Clear();

            foreach (string cardName in cards.Keys)
            {
                BitmapImage bmpImg = new BitmapImage();
                bmpImg.BeginInit();
                bmpImg.UriSource = new Uri("pack://application:,,,/7W;component/Resources/Images/cards/" + cardName + ".jpg");
                bmpImg.EndInit();

                Image img = new Image();
                img.Source = bmpImg;
                img.Height = hand.Height;

                ListBoxItem entry = new ListBoxItem();
                entry.Name = cardName;
                entry.Content = img;

                hand.Items.Add(entry);
            }

            // btnDraft.IsEnabled = false;
        }

        private void hand_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (hand.SelectedItem != null)
            {
                LeaderDescription.Text = coordinator.FindCard(((ListBoxItem)hand.SelectedItem).Name).description;
            }
            else
            {
                LeaderDescription.Text = null;
            }
        }

        private void RecruitedLeaders_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RecruitedLeaders.SelectedItem != null)
            {
                DraftedLeaderDescription.Text = coordinator.FindCard(((ListBoxItem)RecruitedLeaders.SelectedItem).Name).description;
            }
            else
            {
                DraftedLeaderDescription.Text = null;
            }
        }

        private void hand_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListBoxItem entry = hand.SelectedItem as ListBoxItem;

            if (isCourtesanSelection)
            {
                coordinator.copiedLeader = coordinator.FindCard(entry.Name);
                Close();
            }
            else
            {
                hand.Items.Remove(entry);
                RecruitedLeaders.Items.Add(entry);

                if (hand.Items.Count == 0)
                {
                    // if this was the 4th leader to be drafted, or the choice of leader for
                    // the Courtesan's Guild was made, close the dialog box.
                    coordinator.copiedLeader = coordinator.FindCard(entry.Name);
                    Close();
                }
            }

            coordinator.sendToHost(string.Format("BldStrct&Structure={0}", entry.Name));
            coordinator.endTurn();
        }
    }
}

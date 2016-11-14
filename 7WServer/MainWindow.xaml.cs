using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using NLog;

namespace SevenWonders
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static Logger logger = LogManager.GetLogger("SevenWondersServer");
        GMCoordinator gmCoordinator;

        public MainWindow()
        {
            InitializeComponent();

            Closing += MainWindow_Closing;

            Content = "";

            logger.Info("Seven Wonders server application started.");

            gmCoordinator = new GMCoordinator();

            /*
            // TODO: test whether we can use other names, such as "James", "Mike", "Greg", "Ricky", "John", "Kevin"
            StatusChangedEventArgs cmd = new StatusChangedEventArgs("James", "");

            cmd.message = "JJames"; gmCoordinator.receiveMessage(null, cmd);    // James joins the table
            cmd.message = "aa4"; gmCoordinator.receiveMessage(null, cmd);       // Add AI player
            cmd.message = "aa4"; gmCoordinator.receiveMessage(null, cmd);       // Add AI player
            cmd.message = "R"; gmCoordinator.receiveMessage(null, cmd);         // Player is ready.  After all non-AI players send this, the game begins.
            cmd.message = "U"; gmCoordinator.receiveMessage(null, cmd);         // UI is ready to accept the first update
            cmd.message = "r"; gmCoordinator.receiveMessage(null, cmd);         // ready for the first hand of cards
            cmd.message = "BldStrct&WonderStage=0&Structure=Clay Pit"; gmCoordinator.receiveMessage(null, cmd);
            cmd.message = "t"; gmCoordinator.receiveMessage(null, cmd);
            cmd.message = "BldStrct&WonderStage=0&Structure=East Trading Post"; gmCoordinator.receiveMessage(null, cmd);
            cmd.message = "t"; gmCoordinator.receiveMessage(null, cmd);
            cmd.message = "BldStrct&WonderStage=0&Structure=Marketplace"; gmCoordinator.receiveMessage(null, cmd);
            cmd.message = "t"; gmCoordinator.receiveMessage(null, cmd);
            */
            /*
            cmd.message = "Discards&Baths"; gmCoordinator.receiveMessage(null, cmd);
            cmd.message = "t"; gmCoordinator.receiveMessage(null, cmd);
            cmd.message = "BWorkshop"; gmCoordinator.receiveMessage(null, cmd);
            cmd.message = "t"; gmCoordinator.receiveMessage(null, cmd);
            cmd.message = "BStone Pit"; gmCoordinator.receiveMessage(null, cmd);
            cmd.message = "t"; gmCoordinator.receiveMessage(null, cmd);
            cmd.message = "SWest Trading Post"; gmCoordinator.receiveMessage(null, cmd);
            cmd.message = "t"; gmCoordinator.receiveMessage(null, cmd);
            */

            /*
            // Resources (single)

                */
        }

        public void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            gmCoordinator.Shutdown();
            // Handle closing logic, set e.Cancel as needed
        }
    }
}

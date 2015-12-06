using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Threading;

// Discussions about how to use Application.MainWindow
// https://msdn.microsoft.com/en-us/library/system.windows.application.mainwindow
// http://stackoverflow.com/questions/3702785/wpf-application-exits-immediately-when-showing-a-dialog-before-startup
// http://stackoverflow.com/questions/1243833/wpf-showdialog-returns-null-immediately-on-second-call
//
// Describes how to go from a simple non-reusable button to something more reusable
// http://blogs.msdn.com/b/knom/archive/2007/10/31/wpf-control-development-3-ways-to-build-an-imagebutton.aspx

// Resource embedding & referencing.  Much better than having to include a bunch of files along with the exe for this
// program.
// http://stackoverflow.com/questions/347614/wpf-image-resources

namespace SevenWonders
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        Coordinator gameCoordinator;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // As it is currently written, this application creates a startup window
            // which needs to be closed before the Main Table dialog box is shown.  This puts a message on
            // the dispatch queue to terminate the application after the first window is closed, which in
            // turn causes MainWindow.ShowDialog() to return before it's closed.  To prevent this , we change
            // the ShutdownMode to OnExpliciShutdown (its default value is OnMainWindowClose).
            //ShutdownMode = ShutdownMode.OnExplicitShutdown;

            /*
            NameValueCollection nv = new NameValueCollection();

            nv.Add("Player1", "5,6,13,15,12,15,15,0,105");
            nv.Add("Player2", "2,0,1,11,14,0,5,0,60");
            nv.Add("Player3", "2,0,1,11,14,0,5,0,60");
            nv.Add("Player4", "2,0,1,11,14,0,5,0,60");
            nv.Add("Player5", "2,0,1,11,14,0,5,0,60");
            nv.Add("Player6", "2,0,1,11,14,0,5,0,60");
            nv.Add("Player7", "2,0,1,11,14,0,5,0,60");

            FinalScore fs = new FinalScore(nv);
            fs.ShowDialog();
            */

            gameCoordinator = new Coordinator();
            gameCoordinator.createGame();

            if (!gameCoordinator.client.Connected)
                return;


            // after createGame we know how many players and therefore how many player panels are required.

            // Create the startup window.  Needs gameCoordinator so it knows how many players to create.
            MainWindow wnd = new MainWindow(gameCoordinator);

            this.MainWindow = wnd;

            //make graphics better
            RenderOptions.SetBitmapScalingMode(MainWindow, BitmapScalingMode.Fant);

            gameCoordinator.SetMainWindow(wnd);

            // tell the server to start sending game state data now that the Main Window is ready
            gameCoordinator.sendToHost("U");

            // Show the window
            wnd.ShowDialog();

            Application.Current.Shutdown(0);
        }
    }
}

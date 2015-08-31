using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimelineMe.Views;
using Windows.UI.Xaml.Controls;

namespace TimelineMe
{
    public partial class MainPage : Page
    {
        public const string FEATURE_NAME = "Timeline Me";

        List<Scenario> scenarios = new List<Scenario>
        {
            new Scenario() { Title="Start capturing photos/videos", ClassType=typeof(CameraScenario)},
            new Scenario() { Title="Here you can explore your captured videos", ClassType=typeof(StudioScenario)},
        };
    }

    public class Scenario
    {
        public string Title { get; set; }
        public Type ClassType { get; set; }
    }
}


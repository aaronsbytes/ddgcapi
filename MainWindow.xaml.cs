using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using DuckDuckGo;

namespace DDGChatAPI
{
    public partial class MainWindow : Window
    {
        // Create api instance
        ChatAPI api = new ChatAPI();

        // Create markdown engine
        MdXaml.Markdown md = new MdXaml.Markdown();

        public MainWindow()
        {
            InitializeComponent();
        }

        public void clearMarkdownRenderer()
        {
            // Clear markdown renderer
            FlowDocument flowDoc = md.Transform("");
            mdViewer.Document = flowDoc;
        }

        private async void mainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Initialize Conversation
            int init = await api.initConversation();
            if (init != 0)
            {
                MessageBox.Show("Init returned: " + init.ToString());
            }

            // Enable controls after initialization
            chatContentBox.IsEnabled = true;
            sendBTN.IsEnabled = true;

            // Default prompt
            string intro = await api.PromptAsync(api.DefaultPrompt);
            FlowDocument flowDoc = md.Transform(intro);
            mdViewer.Document = flowDoc;
        }

        public async Task sendMessage()
        {
            // Get question from chatbox
            string prompt = chatContentBox.Text;

            // Check if prompt is empty
            if (prompt == null || prompt == "" || prompt == " ")
            {
                return;
            }

            // Add current token to title
            this.Title = "Duck.AI | " + api.token;

            // Clear markdown renderer
            clearMarkdownRenderer();

            // Show loading animation
            loadingAnimation.Visibility = Visibility.Visible;

            // Clear chatbox
            chatContentBox.Clear();

            // Send question to api and wait for response
            var answer = await api.PromptAsync(prompt);

            // Create a FlowDocument for the markdown renderer
            FlowDocument flowDoc = md.Transform(answer.ToString());

            // Hide loading animation
            loadingAnimation.Visibility = Visibility.Hidden;

            // Render the FlowDocument
            mdViewer.Document = flowDoc;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await sendMessage();
        }

        public async void chatBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await sendMessage();
            }
        }

        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
            await sendMessage();
        }
    }
}
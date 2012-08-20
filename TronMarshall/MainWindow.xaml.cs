using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TronSdk;
using System.Timers;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using System.Reflection;

namespace TronMarshall
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        BlockTypes[,] _state; //holds the current game state

        //references the System.Type for the bots. the bot object is instanced for each move as this mimics what 
        //will happen in tournament
        Type _bot1Type, _bot2Type;

        //dispatchertimer runs on ui thread meaning we can update ui without dispatcher invokes
        DispatcherTimer _timer;

        //whose turn
        bool _bot1Turn = false;

        //win count
        int _bot1Wins = 0, _bot2Wins = 0;

        public MainWindow()
        {
            InitializeComponent();

            poulateBotList();

            resetGame();
        }

        #region Setup
        private void poulateBotList()
        {
            List<BotListEntry> bots = new List<BotListEntry>();

            //discover other assemblies in the same location as this assembly.
            //Assembly.GetEntryAssembly().GetReferencedAssemblies() won't return bot assemblies because they're only 
            //used via TronSdk.
            foreach (string path in System.IO.Directory.EnumerateFiles(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "*.dll"))
            {
                Assembly assembly = Assembly.LoadFile(path);

                foreach (Type type in assembly.GetTypes())
                {
                    if (type.GetInterface("ITronBot") != null && !type.IsAbstract)
                    {
                        bots.Add(new BotListEntry(type.Name, type));
                    }
                }
            }

            Bot1ComboBox.ItemsSource = bots;
            Bot2ComboBox.ItemsSource = bots;

            if (bots.Count > 0)
            {
                Bot1ComboBox.SelectedIndex = 0;
                Bot2ComboBox.SelectedIndex = 0;
            }

        }

        private bool setBots()
        {
            if ((Bot1ComboBox.SelectedValue == null) || (Bot2ComboBox.SelectedValue == null)) return false;

            _bot1Type = ((BotListEntry)Bot1ComboBox.SelectedValue).Type;
            _bot2Type = ((BotListEntry)Bot2ComboBox.SelectedValue).Type;

            _bot1Turn = true;

            return true;

        }
        #endregion

        #region Game
        private void resetGame()
        {
            _state = new BlockTypes[TronSdk.Common.BOARD_BLOCKS_X, TronSdk.Common.BOARD_BLOCKS_Y];

            Random rnd = new Random();
            int startX = rnd.Next(TronSdk.Common.BOARD_BLOCKS_X);
            int startY = 1 + rnd.Next(TronSdk.Common.BOARD_BLOCKS_Y - 2);

            _state[startX, startY] = BlockTypes.You;
            _state[(startX + Common.BOARD_BLOCKS_X / 2) % Common.BOARD_BLOCKS_X, Common.BOARD_BLOCKS_Y - 1 - startY] = BlockTypes.Opponent;

            // was: _state[TronSdk.Common.BOARD_BLOCKS_X / 4 - 1, TronSdk.Common.BOARD_BLOCKS_Y / 2] = BlockTypes.You;
            // was: _state[TronSdk.Common.BOARD_BLOCKS_X - (TronSdk.Common.BOARD_BLOCKS_X / 4), TronSdk.Common.BOARD_BLOCKS_Y / 2] = BlockTypes.Opponent;

            _bot1Turn = true;

            updateGridUi();
        }
        #endregion

        #region Timer
        private void stopTimer()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer = null;
            }
        }

        private void startTimer()
        {
            stopTimer();

            _timer = new DispatcherTimer();
            _timer.Interval = new TimeSpan(0, 0, 0, 0, int.Parse(((ComboBoxItem)TurnDelayComboBox.SelectedValue).Content.ToString()));
            _timer.Tick += new EventHandler(_timer_Tick);
            _timer.Start();
        }

        void _timer_Tick(object sender, EventArgs e)
        {

            BlockTypes[,] newState = new BlockTypes[Common.BOARD_BLOCKS_X, Common.BOARD_BLOCKS_Y];
            Array.Copy(_state, newState, Common.BOARD_BLOCKS_X * Common.BOARD_BLOCKS_Y);

            ITronBot bot = (ITronBot)Activator.CreateInstance(_bot1Turn ? _bot1Type : _bot2Type);

            //if it's bot 2's turn, flip
            if (!_bot1Turn) flipBlocks(ref newState);

            //invoke the bot
            bot.ExecuteMove(ref newState);

            try
            {
                BlockTypes[,] oldState =new BlockTypes[Common.BOARD_BLOCKS_X,Common.BOARD_BLOCKS_Y];
                Array.Copy(_state, oldState, Common.BOARD_BLOCKS_X * Common.BOARD_BLOCKS_Y);

                if (!_bot1Turn)
                {
                    //flip the comparison state if needed. this ensures that Validation.ValidateMove only needs to worry
                    //about BlockTypes.You*
                    flipBlocks(ref oldState);
                }

                Validation.ValidateMove( oldState, newState);
            }
            catch (Exception ex)
            {
                ErrorLabel.Content = (_bot1Turn ? "LHS Bot" : "RHS Bot") + ": " + ex.Message;
                stopTimer();
            }

            //if it's bot 2's turn, flip back
            if (!_bot1Turn) flipBlocks(ref newState);

            //render the screen
            updateGridUi(_state, newState);

            //validate end of game
            int winnerBot = 0;
            bool endOfGame = Validation.IsEndOfGame(newState, out winnerBot);

            //switch turns
            _bot1Turn = !_bot1Turn;

            //update master state
            _state = newState;

            //handle end of game
            if (endOfGame)
            {
                updateWins(winnerBot);
                resetGame();
            }

        }
        #endregion

        #region UI Events
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            _bot1Wins = 0;
            _bot2Wins = 0;
            updateWins(0);

            stopTimer();
            resetGame();

            ErrorLabel.Content = "";

            if (!setBots())
            {
                MessageBox.Show("Select a Bot!");
                return;
            }

            startTimer();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            stopTimer();
        }

        private void ViewComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GeometryModel == null) return;

            //bind the appropriate view
            switch (ViewComboBox.SelectedIndex)
            {
                case 0:
                    GeometryModel.Geometry = new PlayGridGenerator().FlattenedSphereGeometry;
                    break;

                case 1:
                    GeometryModel.Geometry = new PlayGridGenerator().RectangularGeometry;
                    break;
            }

            updateGridUi();
        }

        private void TurnDelayComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_timer != null) startTimer();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            stopTimer();
        }

        #endregion

        #region UI
        /// <summary>
        /// Updates the entire grid with the current state
        /// </summary>
        private void updateGridUi()
        {
            for (int x = 0; x < Common.BOARD_BLOCKS_X; x++)
                for (int y = 0; y < Common.BOARD_BLOCKS_Y; y++)
                    updateGridUi(x, y, _state[x, y], false);

            this.InvalidateVisual();
        }

        /// <summary>
        /// Updates elements on the grid that are different between the two states passed in
        /// </summary>
        /// <param name="oldState"></param>
        /// <param name="newState"></param>
        /// <param name="invertBots"></param>
        private void updateGridUi(BlockTypes[,] oldState, BlockTypes[,] newState)
        {
            for (int x = 0; x < Common.BOARD_BLOCKS_X; x++)
            {
                for (int y = 0; y < Common.BOARD_BLOCKS_Y; y++)
                {
                    if (oldState[x, y] != newState[x, y])
                    {
                        updateGridUi(x, y, newState[x, y], false);
                    }
                }
            }

            this.InvalidateVisual(); //doesn't refresh without this
        }

        /// <summary>
        /// Updates the specified grid location with the specified cell type
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="type"></param>
        private void updateGridUi(int x, int y, BlockTypes type, bool invalidate)
        {
            MeshGeometry3D mesh = (MeshGeometry3D)GeometryModel.Geometry;

            int textureCoordinateOffset = ((Common.BOARD_BLOCKS_X * y) + x) * 4;

            int typeInt = (int)type;

            double x1 = (typeInt * PlayGridGenerator.TEXTURE_CELL_CX) / PlayGridGenerator.TEXTURE_IMAGE_CX;
            double x2 = ((((typeInt + 1) * PlayGridGenerator.TEXTURE_CELL_CX)) - 1) / PlayGridGenerator.TEXTURE_IMAGE_CX;

            mesh.TextureCoordinates[textureCoordinateOffset] = new System.Windows.Point(x1, 0);
            mesh.TextureCoordinates[textureCoordinateOffset + 1] = new System.Windows.Point(x2, 0);
            mesh.TextureCoordinates[textureCoordinateOffset + 2] = new System.Windows.Point(x2, 1);
            mesh.TextureCoordinates[textureCoordinateOffset + 3] = new System.Windows.Point(x1, 1);

            if (invalidate) this.InvalidateVisual();
        }

        private void updateWins(int winnerBot)
        {
            if (winnerBot == 1) 
                _bot1Wins++;
            if (winnerBot == 2) 
                _bot2Wins++;

            Bot1WinsLabel.Content = _bot1Wins.ToString();
            Bot2WinsLabel.Content = _bot2Wins.ToString();
        }
        #endregion

        #region Helpers
        private void flipBlocks(ref BlockTypes[,] state)
        {
            for (int x = 0; x < Common.BOARD_BLOCKS_X; x++)
            {
                for (int y = 0; y < Common.BOARD_BLOCKS_Y; y++)
                {
                    state[x, y] = invertBlock(state[x, y]);

                }
            }
        }

        private BlockTypes invertBlock(BlockTypes blockType)
        {
            switch (blockType)
            {
                case BlockTypes.You: return BlockTypes.Opponent;
                case BlockTypes.YourWall: return BlockTypes.OpponentWall;
                case BlockTypes.Opponent: return BlockTypes.You;
                case BlockTypes.OpponentWall: return BlockTypes.YourWall;
                default: return BlockTypes.Clear;
            }
        }
        #endregion





    }
}

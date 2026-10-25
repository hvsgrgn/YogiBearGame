using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using YogiBearAvaloniaCore.Model;
using YogiBearAvaloniaCore.Persistence;

namespace YogiBear.Test
{
    [TestClass]
    public class GameModelTest : IDisposable
    {
        private GameModel _model = null!;
        private GameBoard _mockedBoard = null!;
        private List<Ranger> _mockedRangers = null!;
        private Mock<IDataAccess> _mockDataAccess = null!;
        private int _basketCount = 2;
        private MockTimer _mockTimer = new MockTimer();

        [TestInitialize]
        public void Initialize()
        {
            _mockedBoard = new GameBoard(3);

            // Kosarak elhelyezese
            _mockedBoard.SetField(0, 1, Fillers.Basket);  // Kezdoponttol jobbra
            _mockedBoard.SetField(1, 2, Fillers.Basket);  // Kezdoponttol jobbra 2szer-le

            // Akadalyok elhelyezese
            _mockedBoard.SetField(0, 2, Fillers.Tree);    // jobb felso sarok
            _mockedBoard.SetField(2, 0, Fillers.Bush);    // bal also sarok

            //maradek cellakba ures ertekek
            _mockedBoard.SetField(0, 0, Fillers.None);
            _mockedBoard.SetField(1, 0, Fillers.None);
            _mockedBoard.SetField(1, 1, Fillers.None);
            _mockedBoard.SetField(2, 1, Fillers.None);
            _mockedBoard.SetField(2, 2, Fillers.None);

            // Egyetlen ranger, aki nem mozog a timer elinditasaig
            _mockedRangers = new List<Ranger>
            {
                new Ranger(2, 2, 0, -1)  // jobb also sarok, balra mozog
            };

            // Mock data access
            _mockDataAccess = new Mock<IDataAccess>();
            var mockedRangerTuples = new List<(int, int, int, int)>{
                    (1, 2, 1, 0),
                    (3, 4, 0, 1)
                    };

            _mockDataAccess.Setup(da => da.LoadAsync(It.IsAny<string>()))
                .Returns(Task.FromResult((_mockedBoard, mockedRangerTuples, _basketCount)));

            _model = new GameModel(_mockedBoard, _mockedRangers, _basketCount);

            // Feliratkozasok
            _model.GameAdvanced += Model_GameAdvanced;
            _model.GameWon += Model_GameWon;
            _model.GameLost += Model_GameLost;
        }

        [TestMethod]

        //leteszteljuk, megfelelo-e az inicializalas
        public void GameModelNewGameTest()
        {
            _model.NewGame();

            Assert.AreEqual(0, _model.GameTime); 
            Assert.AreEqual(0, _model.CollectedBasketCount);
            Assert.AreEqual(_basketCount, _model.AllBasketCount);
            Assert.AreEqual(3, _model.GameBoardSize);
            Assert.IsTrue(_model.IsYogiAt(0, 0), "Yogi should start at (0,0)");
        }

        [TestMethod]
        //megnezzuk, jol mukodik e maci laci mozgatasa
        public void GameModelMoveYogiTest()
        {
            _model.NewGame();

            // Test 1: Jobbra levo kosarat fel kell vennie
            _model.MoveYogiBear(0, 1);
            Assert.IsTrue(_model.IsYogiAt(0, 1), "Yogi should be at (0,1) after moving right");
            Assert.AreEqual(1, _model.CollectedBasketCount, "Should collect basket at (0,1)");

            // Test 2: Lefele lepes - itt nem kap el a ranger, mert nem megy a timer
            _model.MoveYogiBear(1, 0);
            Assert.IsTrue(_model.IsYogiAt(1, 1), "Yogi should be at (1,1) after moving down");
            Assert.AreEqual(1, _model.CollectedBasketCount, "Should still have 1 basket (empty field)");

            // Test 3: Jobbra a kosar
            _model.MoveYogiBear(0, 1);
            Assert.IsTrue(_model.IsYogiAt(1, 2), "Yogi should be at (1,2) after moving right");
            Assert.AreEqual(2, _model.CollectedBasketCount, "Should collect basket at (1,2)");

            // Test 4: Felfele fa van, ezt a lepest nem szabad vegrehajtania
            _model.MoveYogiBear(-1, 0);
            Assert.IsTrue(_model.IsYogiAt(1, 2), "Should not move into tree");
            Assert.AreEqual(2, _model.CollectedBasketCount, "Basket count should not change");
        }

        [TestMethod]
        // most a rangermozgast is nezni kell
        public void GameModelRangerMovementTest()
        {
            _model.NewGame();

            Ranger testRanger = new Ranger(2, 2, 0, -1);

            Assert.IsTrue(_model.isValidMove(1,2), "Ranger should be able to step onto empty field");
            testRanger.Move();
            Assert.AreEqual(2, testRanger.CoordsX, "Ranger should stay at old x");
            Assert.AreEqual(1, testRanger.CoordsY, "Ranger should move in direction");

            //a fa miatt valtozatlan pozicoja kell, hogy legyen, de forditott irany
            Assert.IsFalse(_model.isValidMove(2,0), "Ranger should not be able to step onto tree field");

            testRanger.Turn();
            Assert.AreEqual(2, testRanger.CoordsX, "Ranger should stay at old x");
            Assert.AreEqual(1, testRanger.CoordsY, "Ranger should stay at old y");
            Assert.AreEqual(1, testRanger.DirectionY);

            testRanger.Move();
            Assert.AreEqual(2, testRanger.CoordsX, "Ranger should stay at old x");
            Assert.AreEqual(2, testRanger.CoordsY, "Ranger should move in direction");

        }
        [TestMethod]
        public void GameModelRangerMovesAndCatchesYogiTest()
        {
            _model.NewGame();

            bool gameLostEventFired = false;
            _model.GameLost += (sender, e) => { gameLostEventFired = true; };

            _model.MoveYogiBear(0, 1);
            _model.MoveYogiBear(1, 0);
            _mockTimer.RaiseElapsed();
            Assert.IsTrue(gameLostEventFired, "GameLost event should have been fired when ranger caught Yogi");
        }



        [TestMethod]
        public void GameModelWinConditionTest()
        {
            _model.NewGame();

            bool gameWonEventFired = false;
            _model.GameLost += (sender, e) => { gameWonEventFired = true; };

            _model.MoveYogiBear(0, 1); // Kosar (0,1)
            Assert.AreEqual(1, _model.CollectedBasketCount);

            _model.MoveYogiBear(1, 1); // Kosar (1,2)  
            Assert.AreEqual(2, _model.CollectedBasketCount);

            //yogi biztonsagba mozgatasa
            _model.MoveYogiBear(-1, -1);
            _mockTimer.RaiseElapsed();

            Assert.AreEqual(_model.AllBasketCount, _model.CollectedBasketCount, "Should have all baskets collected");
            Assert.IsTrue(gameWonEventFired, "GameWon event should have been fired when all baskets were collected");
        }

        [TestMethod]
        public void GameModelPauseResumeTest()
        {
            _model.NewGame();

            // Megnezzuk, nem valtodik-e ki exception
            _model.PauseGame();
            _model.ResumeGame();
        }

        [TestMethod]
        public async Task GameModelDataAccessTest()
        {
            var (board, rangers, baskets) = await _mockDataAccess.Object.LoadAsync("test.txt");

            Assert.IsNotNull(board);
            Assert.IsNotNull(rangers);
            Assert.AreEqual(_basketCount, baskets);
            _mockDataAccess.Verify(da => da.LoadAsync("test.txt"), Times.Once());
        }

        [TestMethod]
        public void GameModelFieldAccessTest()
        {
            _model.NewGame();

            // Teszteljuk, hogy elerjuk-e a modell indexelesevel a mezoket
            Assert.AreEqual(Fillers.Basket, _model[0, 1], "Field (0,1) should contain basket");
            Assert.AreEqual(Fillers.Tree, _model[0, 2], "Field (0,2) should contain tree");
            Assert.AreEqual(Fillers.None, _model[1, 1], "Field (1,1) should be empty");
        }

        private void Model_GameAdvanced(object? sender, EventArgs e)
        {
            //Csak arra van, hogy legyen mire feliratkozni
        }

        private void Model_GameWon(object? sender, EventArgs e)
        {
            //Csak arra van, hogy legyen mire feliratkozni  
        }

        private void Model_GameLost(object? sender, EventArgs e)
        {
            //Csak arra van, hogy legyen mire feliratkozni
        }

        public void Dispose() {
            _mockTimer.Dispose();
        }
    }
}

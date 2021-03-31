using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GTANetworkAPI;

namespace SouthValleyFive.Scripts.Poker
{
    public class PokerTableManager : Script
    {
        public static List<PokerTableManager> pokerTables = new List<PokerTableManager>();
        private PlayerList activePlayers = new PlayerList();
        private PlayerList SeatedPlayers = new PlayerList();
        private int MaxPlayers;
        private Deck CardDeck;
        private Hand TableHand = new Hand();
        private int RoundCounter;
        private readonly Pot MainPot;
        private readonly List<Pot> SidePots;
        private readonly Random Rand;
        private int TurnCounter;
        public string Winnermessage;
        private bool IsActive;
        private bool playerPlaying;
        private CancellationTokenSource source = new CancellationTokenSource();
        private CancellationToken token;
        private int id;
        //the blind class, containing the amount of blinds, the position of the player
        //who must pay the blinds
        private class Blind
        {
            private int _amount;
            public int Position;
            public int Amount
            {
                get
                {
                    return _amount;
                }
                set
                {
                    _amount = value;
                }
            }

        }

        readonly Blind _smallBlind;

        readonly Blind _bigBlind;

        //the index of the player who's the dealer
        private int _dealerPosition;
        //the index of the current player who turn it is
        private int _currentIndex;
        private bool playing;

        public void setActivePlayers(PlayerList pokerPlayers)
        {
            foreach (PokerPlayer player in pokerPlayers)
            {
                NAPI.Util.ConsoleOutput("Added "+player.Name+" To Active Players.");
                activePlayers.Add(player);
            }
        }
        

        //various propeties
        public int TurnCount
        {
            get { return TurnCounter; }
            set { TurnCounter = value; }
        }
        public int SmallBlind
        {
            get { return _smallBlind.Amount; }
        }
        public int BigBlind
        {
            get { return _bigBlind.Amount; }
        }
        public int RoundCount
        {
            get { return RoundCounter; }
            set { RoundCounter = value; }
        }
        /// <summary>
        /// contructor to begin the game, dealer position (and big/small blind position) is randomly choosen
        /// blinds are set to &500/1000 initially 
        /// </summary>
        /// <param name="players"></param>

        //parameterless constructor
        public PokerTableManager()
        {

        }
        public PokerTableManager(PlayerList players, int maxPlayers, int id)
        {
            token = source.Token;
            this.id = id;
            this.SeatedPlayers = players;
            MaxPlayers = maxPlayers;
            CardDeck = new Deck();
            Rand = new Random();
            MainPot = new Pot();
            SidePots = new List<Pot>();
            _smallBlind = new Blind();
            _bigBlind = new Blind();
            RoundCounter = 0;
            TurnCounter = 0;
            _dealerPosition = Rand.Next(players.Count);
            //set blind amount and position
            _smallBlind.Amount = 500;
            _bigBlind.Amount = 1000;
            MainPot.SmallBlind = 500;
            MainPot.BigBlind = 1000;
            _smallBlind.Position = _dealerPosition + 1;
            _bigBlind.Position = _dealerPosition + 2;
            _currentIndex = _dealerPosition;
            this.IsActive = false;
            this.playerPlaying = false;
        }
        public PokerTableManager(int maxPlayers, int id)
        {
            this.id = id;
            MaxPlayers = maxPlayers;
            SeatedPlayers = new PlayerList();
            activePlayers = new PlayerList();
            CardDeck = new Deck();
            Rand = new Random();
            MainPot = new Pot();
            SidePots = new List<Pot>();
            _smallBlind = new Blind();
            _bigBlind = new Blind();
            RoundCounter = 0;
            TurnCounter = 0;
            _dealerPosition = Rand.Next(activePlayers.Count);
            //set blind amount and position
            _smallBlind.Amount = 500;
            _bigBlind.Amount = 1000;
            MainPot.SmallBlind = 500;
            MainPot.BigBlind = 1000;
            _smallBlind.Position = _dealerPosition + 1;
            _bigBlind.Position = _dealerPosition + 2;
            _currentIndex = _dealerPosition;
        }
        //indexer of players
        public PokerPlayer this[int index]
        {
            get
            {
                return activePlayers.GetPlayer(ref index);
            }
            set
            {
                activePlayers[index] = value;
            }
        }

        //various getters/setters
        public PlayerList GetPlayers()
        {
            return activePlayers;
        }
        public int GetDealerPosition()
        {
            return _dealerPosition;
        }
        public int GetCurrentIndex()
        {
            return _currentIndex;
        }
        public void SetCurrentIndex(int index)
        {
            _currentIndex = index;
        }
        public string GetSmallBlind()
        {
            return _smallBlind.Amount.ToString();
        }
        public string GetBigBlind()
        {
            return _bigBlind.Amount.ToString();
        }
        public Pot GetPot()
        {
            return MainPot;
        }
        public List<Pot> GetSidePots()
        {
            return SidePots;
        }
        public Hand GetCommunityCards()
        {
            return TableHand;
        }
        public Deck GetDeck()
        {
            return CardDeck;
        }
        /// <summary>
        /// Remove a player when the player busts out.
        /// </summary>
        /// <param name="player"></param>
        public void RemovePlayer(PokerPlayer player)
        {
            if (player.ChipStack != 0)
                throw new InvalidOperationException();
            SeatedPlayers.Remove(player);
        }
        public void RemovePlayer(int index)
        {
            if (SeatedPlayers[index].ChipStack != 0)
                throw new InvalidOperationException();
            SeatedPlayers.RemoveAt(index);
        }
        [RemoteEvent("JoinPokerMatch")]
        public void JoinMatch(Player player, int amount)
        {
            try
            {
                NAPI.Task.Run(() =>
                {
                    if (SeatedPlayers.Count > MaxPlayers)
                    {
                        player.TriggerEvent("showPokerErrorMessage", 0);
                        return;
                    }

                    SeatedPlayers.Add(new PokerPlayer(player.Name, amount, player));
                    NAPI.Util.ConsoleOutput(player.Name + " JOINED TABLE, PLAYER COUNT = " + SeatedPlayers.Count + " ID TABLE = " + id);
                    string json = "{'fiches': " + amount + ", 'pot': " + MainPot.Amount + ", 'playerName': '" + player.Name + "', 'tableCards': '" + TableHand + "'}";

                    if (SeatedPlayers.Count >= 2 && !IsActive)
                    {

                        player.TriggerEvent("JoinTable", json);
                        StartNextMatch();
                        player.TriggerEvent("StartNextMatch");
                      
                    }
                    else
                    {
                        player.TriggerEvent("JoinTable", json);
                    }

                });
            }
            catch (Exception e)
            {
                NAPI.Util.ConsoleOutput(e.StackTrace);
            }

        }
        [RemoteEvent("pokerLeave")]
        public void PokerLeaveEvent(Player player)
        {
            try
            {
                PokerTableManager pokerTableManager = null;
                PokerPlayer playerPoker = null;

                foreach (PokerTableManager manager in pokerTables)
                {
                    foreach (PokerPlayer pokerPlayer in manager.activePlayers)
                    {
                        if (pokerPlayer.Name == player.Name)
                        {
                            pokerTableManager = manager;
                            playerPoker = pokerPlayer;
                        }
                    }
                }
                //This is an abomination.
                poker.Poker.OnPlayerLeaveTable(player);
                //TODO: Empty the seat the player was occupying.

            }
            catch (Exception e)
            {
                NAPI.Util.ConsoleOutput(e.StackTrace);
            }

        }
        [RemoteEvent("PokerCallEvent")]
        public void PlayerCall(Player player)
        {
            try
            {
                PokerTableManager pokerTableManager = null;
                PokerPlayer playerPoker = null;

                foreach (PokerTableManager manager in pokerTables)
                {
                    foreach (PokerPlayer pokerPlayer in manager.activePlayers)
                    {
                        if (pokerPlayer.Name == player.Name)
                        {
                            pokerTableManager = manager;
                            playerPoker = pokerPlayer;
                        }
                    }
                }

                foreach(PokerPlayer pokerPlayer in pokerTableManager.SeatedPlayers){

                    if(pokerPlayer.Name != player.Name)
                    {
                        pokerPlayer.playerObject.SendChatMessage(player.Name+" Ha fatto Call.");
                    }

                }

                playerPoker.playerObject.SendChatMessage("Hai fatto Call.");
                pokerTableManager.playerPlaying = false;
                pokerTableManager.source.Cancel();
                NAPI.Util.ConsoleOutput("Cancelled Timeout.");
                // Need to check if it's their turn.


                playerPoker.Call(pokerTableManager.MainPot);
                // CLIENTSIDE -> FRONTEND INTERFACE
                updateraises(pokerTableManager);
                player.TriggerEvent("OnPlayerPlayed", "{'updatedPot': " + pokerTableManager.MainPot.Amount + ", 'action': 'Call'}");
                //Browser.ExecuteJsFunction("OnPlayerRaiseUpdated({minRaise:"+ MainPot.MinimumRaise +", maxRaise:"+ MainPot.getMaximumAmountPutIn() +"});");
                //Browser.ExecuteJsFunction("OnPlayerPlayed({updatedPot:"+ MainPot.Amount +", action:'Call'});");
            }
            catch (Exception e)
            {
                NAPI.Util.ConsoleOutput(e.StackTrace);
            }

        }
        [RemoteEvent("PokerRaiseEvent")]
        public void PlayerRaise(Player player, int fiches)
        {
            try
            {
                // Need to check if it's their turn.
                PokerTableManager pokerTableManager = null;
                PokerPlayer playerPoker = null;

                foreach (PokerTableManager manager in pokerTables)
                {
                    foreach (PokerPlayer pokerPlayer in manager.activePlayers)
                    {
                        if (pokerPlayer.Name == player.Name)
                        {
                            pokerTableManager = manager;
                            playerPoker = pokerPlayer;
                        }
                    }
                }

                foreach (PokerPlayer pokerPlayer in pokerTableManager.SeatedPlayers)
                {

                    if (pokerPlayer.Name != player.Name)
                    {
                        pokerPlayer.playerObject.SendChatMessage(player.Name + " Ha fatto un raise di "+ fiches+"$.");
                    }

                }

                playerPoker.Raise(fiches, pokerTableManager.MainPot);
                pokerTableManager.MainPot.AgressorIndex = pokerTableManager._currentIndex;

                NAPI.Util.ConsoleOutput("Aggressor index is now "+ pokerTableManager._currentIndex);
                playerPoker.playerObject.SendChatMessage("Hai Fatto un raise di " + fiches + ".");
                pokerTableManager.playerPlaying = false;
                pokerTableManager.source.Cancel();
                NAPI.Util.ConsoleOutput("Cancelled Timeout.");


                // CLIENTSIDE -> FRONTEND INTERFACE
                updateraises(pokerTableManager);
                player.TriggerEvent("OnPlayerPlayed", "{'updatedPot': " + pokerTableManager.MainPot.Amount + ", 'action': 'Raise'}");
            }
            catch (Exception e)
            {
                NAPI.Util.ConsoleOutput(e.StackTrace);
            }

        }

        private void updateraises(PokerTableManager pokerTableManager)
        {
            int amount = 0;
            foreach(PokerPlayer player in pokerTableManager)
            {
                amount = (pokerTableManager.MainPot.MinimumRaise*2)-player.AmountInPot;
                player.playerObject.TriggerEvent("OnPlayerRaiseUpdated", "{'minRaise': " + pokerTableManager.MainPot.MinimumRaise + ", 'maxRaise': " + player.ChipStack + "}");
            }
        }

        /*mp.events.add({
   "PokerRaiseEvent": fiches => {
       PlayerRaise(fiches)
   },
})*/
        [RemoteEvent("PokerFoldEvent")]
        public void PlayerFold(Player player)
        {
            try
            {
                // Need to check if it's their turn.
                PokerTableManager pokerTableManager = null;
                PokerPlayer playerPoker = null;

                foreach (PokerTableManager manager in pokerTables)
                {
                    foreach (PokerPlayer pokerPlayer in manager.activePlayers)
                    {
                        if (pokerPlayer.Name == player.Name)
                        {
                            pokerTableManager = manager;
                            playerPoker = pokerPlayer;
                        }
                    }
                }

                foreach (PokerPlayer pokerPlayer in pokerTableManager.SeatedPlayers)
                {

                    if (pokerPlayer.Name != player.Name)
                    {
                        pokerPlayer.playerObject.SendChatMessage(player.Name + " Ha foldato.");
                    }

                }

                playerPoker.Fold(pokerTableManager.MainPot);
                playerPoker.folded = true;
                playerPoker.playerObject.SendChatMessage("Hai Foldato.");
                pokerTableManager.activePlayers[GetCurrentIndex()].folded = true;

                NAPI.Util.ConsoleOutput("Player Folded. == "+pokerTableManager.activePlayers[GetCurrentIndex()].folded);


                NAPI.Util.ConsoleOutput("Player Folded.");
                pokerTableManager.playerPlaying = false;
                pokerTableManager.source.Cancel();
                NAPI.Util.ConsoleOutput("Cancelled Timeout.");


                //Set bool playing to false

                // CLIENTSIDE -> FRONTEND INTERFACE
                updateraises(pokerTableManager);
                player.TriggerEvent("OnPlayerPlayed", "{'updatedPot': " + pokerTableManager.MainPot.Amount + ", 'action': 'Fold'}");
            }
            catch (Exception e)
            {
                NAPI.Util.ConsoleOutput(e.StackTrace);
            }


        }

        /// <summary>
        /// Start a new round, dealer/smallblind position are moved up one spot
        /// players/counter variables are reset
        /// blinds are reset if necessary.
        /// </summary>
        public async void StartNextMatch()
        {
            IsActive = true;
            await WaitGameStart();
            setActivePlayers(SeatedPlayers);
            activePlayers.ResetPlayers();
            CardDeck = new Deck();
            if (RoundCounter == 10)
            {
                RoundCounter = 0;
                _smallBlind.Amount *= 2;
                _bigBlind.Amount = _smallBlind.Amount * 2;
                MainPot.SmallBlind = SmallBlind;
                MainPot.BigBlind = BigBlind;
            }
            if (RoundCounter != 0)
            {
                _dealerPosition = IncrementIndex(_dealerPosition);
                _smallBlind.Position = IncrementIndex(_dealerPosition);
                _bigBlind.Position = IncrementIndex(_smallBlind.Position);
            }
            RoundCounter++;
            MainPot.Amount = 0;
            MainPot.AgressorIndex = -1;
            MainPot.MinimumRaise = _bigBlind.Amount;
            TableHand.Clear();
            _currentIndex = _dealerPosition;
            Winnermessage = null;
            MainPot.getPlayersInPot().Clear();
            SidePots.Clear();
            foreach (PokerPlayer pokerPlayer in activePlayers)
            {
                ////Browser.ExecuteJsFunction($"StartNextMatch();");
                pokerPlayer.playerObject.TriggerEvent("showPokerMessage", "La partita sta per cominciare...");
                pokerPlayer.playerObject.TriggerEvent("StartNextMatch");
            }
            PokerGame();
        }

        public async Task PokerGame()
        {
            try
            {
                // OPENING DEAL
                //Pay the small and big blind, bringing currentidex to Big Blind

                foreach (PokerPlayer pokerPlayer in activePlayers)
                {

                    pokerPlayer.playerObject.SendChatMessage(activePlayers[_currentIndex + 1].playerObject.Name + " Ha pagato " + _smallBlind.Amount + " di piccolo Buio");

                    pokerPlayer.playerObject.SendChatMessage(activePlayers[_currentIndex + 2].playerObject.Name + " Ha pagato " + _bigBlind.Amount + " di grande Buio");

                }

                PaySmallBlind();

                PayBigBlind();

                CardDeck.Shuffle();

                DealHoleCards();

                //FIRST ROUND OF BETTING
                //Get player starting from first after big blind, do turns        
                await FirstBetAsync();

                //THE FLOP
                //The dealer burns a card, and then deals three community cards face up.
                //The first three cards are referred to as the flop, while all of the community cards are collectively called the board. 

                //Burn a card
                CardDeck.Deal();
                //Add three cards to table
                TableHand.Add(CardDeck.Deal());
                TableHand.Add(CardDeck.Deal());
                TableHand.Add(CardDeck.Deal());

                foreach (PokerPlayer pokerPlayer in activePlayers)
                {
                    //    pokerPlayer.playerObject.SendChatMessage("Card 1: "+ TableHand[0].getRank() + TableHand[0].getSuit());
                    //    pokerPlayer.playerObject.SendChatMessage("Card 2: " + TableHand[1].getRank() + TableHand[1].getSuit());
                    //    pokerPlayer.playerObject.SendChatMessage("Card 3: " + TableHand[2].getRank() + TableHand[2].getSuit());

                    pokerPlayer.AddToHand(TableHand);

                    pokerPlayer.playerObject.TriggerEvent("AddTableCard", "{'card':" + TableHand[0].getRank() + ",'seed': " + TableHand[0].getSuit() + "}");
                    pokerPlayer.playerObject.TriggerEvent("AddTableCard", "{'card':" + TableHand[1].getRank() + ",'seed': " + TableHand[1].getSuit() + "}");
                    pokerPlayer.playerObject.TriggerEvent("AddTableCard", "{'card':" + TableHand[2].getRank() + ",'seed': " + TableHand[2].getSuit() + "}");

                }

                //SECOND ROUND OF BETTING
                //
                await SecondBetAsync();

                //THE TURN
                //The dealer burns another card, and then adds a fourth card face-up to the community cards.
                //This fourth card is known as the turn card, or fourth street.

                CardDeck.Deal();
                Card turn = CardDeck.Deal();

                TableHand.Add(turn);

                foreach (PokerPlayer pokerPlayer in activePlayers)
                {

                    pokerPlayer.AddToHand(turn);
                    pokerPlayer.playerObject.TriggerEvent("AddTableCard", "{'card':" + TableHand[3].getRank() + ",'seed': " + TableHand[3].getSuit() + "}");

                }

                //THIRD ROUND OF BETTING
                //
                await ThirdBetAsync();

                //THE RIVER
                //The dealer burns another card, and then adds a fifth and final card to the community cards. 
                //This fifth card is known as the river card, or fifth street. 

                CardDeck.Deal();
                Card river = CardDeck.Deal();
                TableHand.Add(river);

                foreach (PokerPlayer pokerPlayer in activePlayers)
                {
                    pokerPlayer.AddToHand(river);
                    pokerPlayer.playerObject.TriggerEvent("AddTableCard", "{'card':" + TableHand[4].getRank() + ",'seed': " + TableHand[4].getSuit() + "}");

                    Hand besthand = HandCombination.getBestHandEfficiently(pokerPlayer.GetHand());
                    pokerPlayer.SetHand(besthand);

                }

                //FINAL ROUND OF BETTING
                //
                await FinalBetAsync();



                //SHOWDOWN
                ShowDown();


            }
            catch (Exception e)
            {
                NAPI.Util.ConsoleOutput(e.StackTrace);
            }

        }

        public async Task FirstBetAsync()
        {
            while (BeginNextTurn())
            {
                IncrementIndex(_currentIndex);
                await TimeOut(token);
            }
            return;
        }

        public async Task SecondBetAsync()
        {
            while (BeginNextTurn())
            {
                IncrementIndex(_currentIndex);
                await TimeOut(token);
            }
            return;
        }

        public async Task ThirdBetAsync()
        {
            while (BeginNextTurn())
            {
                IncrementIndex(_currentIndex);
                await TimeOut(token);
            }
            return;
        }

        public async Task FinalBetAsync()
        {
            while (BeginNextTurn())
            {
                IncrementIndex(_currentIndex);
                await TimeOut(token);
            }
            return;
        }


        //Check if everyone (except foldeds and all ins) have the same amount in pot
        public bool isCurrentBettingRoundOver()
        {
            int index = GetCurrentIndex();
            int offset = 1;

            NAPI.Util.ConsoleOutput("CurrentIndex = "+index+", Aggressor Index = "+MainPot.AgressorIndex+",Player Folded = "+activePlayers[index].folded+", Player Count = "+activePlayers.Count());

            while (activePlayers[MainPot.AgressorIndex].IsFolded() && _currentIndex != MainPot.AgressorIndex)
                MainPot.AgressorIndex = DecrementIndex(MainPot.AgressorIndex);
            if (_currentIndex == MainPot.AgressorIndex && TurnCounter > 1)
            {
                NAPI.Util.ConsoleOutput("false");
                return false;
            }
            else if (EveryoneAllIn())
            {
                NAPI.Util.ConsoleOutput("false");
                return false;
            }
            else
            {
                NAPI.Util.ConsoleOutput("true");
                return true;
            }

            /*while (activePlayers[index].isbusted || activePlayers[index].IsFolded())
            {
                NAPI.Util.ConsoleOutput("Player at "+index+" is Folded/Busted");
                index--;
                index %= activePlayers.Count();
                offset++;
                offset %= activePlayers.Count();
            }

            if((index+offset)%activePlayers.Count == MainPot.AgressorIndex)
            {
                if (activePlayers[index].AmountInPot == activePlayers[MainPot.AgressorIndex].AmountInPot)
                {
                    return true;
                }
            } */


            //return false;
        }



        public async Task WaitGameStart()
        {
            await Task.Delay(10000);
        }

        public async Task TimeOut(CancellationToken cancelToken)
        {
            int seconds = 10; //Time in seconds for timeout
            int count = 0;
            playerPlaying = true;
            while (playerPlaying)
            {
                if (count >= seconds)
                {
                    NAPI.Util.ConsoleOutput("Timed Out.");
                    activePlayers[_currentIndex].Fold(MainPot);
                    activePlayers[_currentIndex].folded = true;

                    playerPlaying = false;
                    return;
                }
                else
                {
                    await Task.Delay(1000, token);
                    count++;
                 //   NAPI.Util.ConsoleOutput(count.ToString());
                }
            }

            for (int i = 0; i < seconds; i++)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }
            }

        }


        /// <summary>
        /// Determine when the current betting round is over
        /// </summary>
        /// <returns></returns>
        public bool BeginNextTurn()
        {
            TurnCounter++;
            while (activePlayers[MainPot.AgressorIndex].IsFolded() && _currentIndex != MainPot.AgressorIndex)
                MainPot.AgressorIndex = DecrementIndex(MainPot.AgressorIndex);
            if (_currentIndex == MainPot.AgressorIndex && TurnCounter > 1)
                return false;
            else if (EveryoneAllIn())
                return false;
            else
            {
                NAPI.Util.ConsoleOutput("Current betting round is over.");
                return true;
            }
                
        }
        //method to determine if every player has already went all in
        public bool EveryoneAllIn()
        {
            int zeroCount = 0;
            int totalCount = 0;
            for (int i = 0; i < GetPlayers().Count; i++)
            {
                if (this[i].isbusted || this[i].IsFolded())
                    continue;
                if (this[i].ChipStack == 0)
                    zeroCount++;
                totalCount++;
            }
            if (zeroCount != 0 && totalCount == zeroCount)
                return true;
            else if (totalCount - zeroCount == 1)
            {
                for (int i = 0; i < GetPlayers().Count; i++)
                {
                    if (this[i].isbusted || this[i].IsFolded())
                        continue;
                    if (this[i].ChipStack != 0 && this[i].GetAmountToCall(MainPot) == 0)
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// increment index, skipping folded players, busted players and supports 
        ///wrapping around classes
        /// </summary>
        /// <param name="currentIndex"></param>
        /// <returns></returns>
        public int IncrementIndex(int currentIndex)
        {
            currentIndex++;
            while (activePlayers.GetPlayer(ref currentIndex).IsFolded() || activePlayers.GetPlayer(ref currentIndex).isbusted || activePlayers.GetPlayer(ref currentIndex).ChipStack == 0)
                currentIndex++;
            Player player = activePlayers.GetPlayer(ref currentIndex).playerObject;
            // CLIENTSIDE -> FRONTEND
            int callValue = activePlayers.GetPlayer(ref currentIndex).GetAmountToCall(MainPot);
            ////Browser.ExecuteJsFunction($"OnPlayerTurn(callValue)");
            ////Browser.ExecuteJsFunction($"ShowCards()");
            player.TriggerEvent("OnPlayerTurn", callValue);
            player.TriggerEvent("ShowCards");
            return currentIndex;
        }
        //increment index, not skipping players with a chipstack of zero
        public int IncrementIndexShowdown(int currentIndex)
        {
            currentIndex++;
            while (activePlayers.GetPlayer(ref currentIndex).IsFolded() || activePlayers.GetPlayer(ref currentIndex).isbusted)
                currentIndex++;
            activePlayers.GetPlayer(ref currentIndex);
            return currentIndex;
        }
        //same as increment class except in the other direction
        public int DecrementIndex(int currentIndex)
        {
            currentIndex--;
            while (activePlayers.GetPlayer(ref currentIndex).IsFolded() || activePlayers.GetPlayer(ref currentIndex).isbusted || activePlayers.GetPlayer(ref currentIndex).ChipStack == 0)
                currentIndex--;
            activePlayers.GetPlayer(ref currentIndex);
            return currentIndex;
        }

        //deal two unique cards to all players
        public void DealHoleCards()
        {
            foreach (PokerPlayer pokerPlayer in activePlayers)
            {
                //Deal pocket cards to players in game
                Card card1 = CardDeck.Deal();
                Card card2 = CardDeck.Deal();

                pokerPlayer.AddToHand(card1);
                pokerPlayer.AddToHand(card2);

                pokerPlayer.playerObject.TriggerEvent("GiveCards",
               "{\"cards\":[[" + card1.getRank() + ","
               + card2.getRank() + "]," +
               "[" + card1.getSuit() + ","
               + card2.getSuit() + "]]}");

                pokerPlayer.playerObject.TriggerEvent("UpdatePot", MainPot.Amount);

            }
        }
        //pay small/big blind amount
        public void PaySmallBlind()
        {
            activePlayers.GetPlayer(ref _smallBlind.Position).PaySmallBlind(_smallBlind.Amount, MainPot, _currentIndex);
            _currentIndex = _smallBlind.Position;
            MainPot.AgressorIndex = _currentIndex;
        }
        public void PayBigBlind()
        {
            activePlayers.GetPlayer(ref _bigBlind.Position).PayBigBlind(_bigBlind.Amount, MainPot, _currentIndex);
            _currentIndex = _bigBlind.Position;
            MainPot.AgressorIndex = _currentIndex;
            TurnCounter = 0;
        }
        //deal the flop
        public void DealFlop(Player player)
        {
            TableHand.Add(CardDeck.Deal());
            TableHand.Add(CardDeck.Deal());
            TableHand.Add(CardDeck.Deal());
            for (int i = 0; i < activePlayers.Count; i++)
            {
                activePlayers[i].AddToHand(TableHand);
            }
            // CLIENTSIDE -> FRONTEND
            /*Hand _deliverTableHand = new Hand();
            _deliverTableHand.Add(TableHand[TableHand.Count-3]);
            _deliverTableHand.Add(TableHand[TableHand.Count-2]);
            _deliverTableHand.Add(TableHand[TableHand.Count-1]);*/
            ////Browser.ExecuteJsFunction($"AddTableCard(" + TableHand[TableHand.Count()-3] + ");");
            ////Browser.ExecuteJsFunction($"AddTableCard(" + TableHand[TableHand.Count()-2] + ");");
            ////Browser.ExecuteJsFunction($"AddTableCard(" + TableHand[TableHand.Count()-1] + ");");
            player.TriggerEvent("AddTableCard", "{hand: " + TableHand[TableHand.Count() - 3] + "}");
            player.TriggerEvent("AddTableCard", "{hand: " + TableHand[TableHand.Count() - 2] + "}");
            player.TriggerEvent("AddTableCard", "{hand: " + TableHand[TableHand.Count() - 1] + "}");
        }
        //deal the turn
        public void DealTurn(Player player)
        {
            Card turn = CardDeck.Deal();
            TableHand.Add(turn);
            for (int i = 0; i < activePlayers.Count; i++)
            {
                activePlayers[i].AddToHand(turn);
            }
            // CLIENTSIDE -> FRONTEND
            ////Browser.ExecuteJsFunction($"AddTableCard(" + TableHand[TableHand.Count()-1] + ");");
            player.TriggerEvent("AddTableCard", "{hand: " + TableHand[TableHand.Count() - 1] + "}");
        }
        //deal the river
        public void DealRiver(Player player)
        {
            Card river = CardDeck.Deal();
            TableHand.Add(river);
            for (int i = 0; i < activePlayers.Count; i++)
            {
                activePlayers[i].AddToHand(river);
            }
            // CLIENTSIDE -> FRONTEND
            ////Browser.ExecuteJsFunction($"AddTableCard(" + TableHand[TableHand.Count()-1] + ");");
            player.TriggerEvent("AddTableCard", "{hand: " + TableHand[TableHand.Count() - 1] + "}");

        }
        //showdown code!
        public void ShowDown()
        {
            //creating sidepots
            if (CreateSidePots())
            {
                MainPot.getPlayersInPot().Sort();

                for (int i = 0; i < MainPot.getPlayersInPot().Count - 1; i++)
                {
                    if (MainPot.getPlayersInPot()[i].AmountInPot != MainPot.getPlayersInPot()[i + 1].AmountInPot)
                    {
                        PlayerList tempPlayers = new PlayerList();
                        for (int j = MainPot.getPlayersInPot().Count - 1; j > i; j--)
                        {
                            tempPlayers.Add(MainPot.getPlayersInPot()[j]);
                        }
                        int potSize = (MainPot.getPlayersInPot()[i + 1].AmountInPot - MainPot.getPlayersInPot()[i].AmountInPot) * tempPlayers.Count;
                        MainPot.Amount -= potSize;
                        SidePots.Add(new Pot(potSize, tempPlayers));
                    }
                }
            }
            //awarding mainpot
            PlayerList bestHandList = new PlayerList();
            List<int> winners = new List<int>();
            bestHandList = QuickSortBestHand(new PlayerList(MainPot.getPlayersInPot()));
            for (int i = 0; i < bestHandList.Count; i++)
            {
                for (int j = 0; j < this.GetPlayers().Count; j++)
                {
                    if (activePlayers[j] == bestHandList[i])
                    {
                        winners.Add(j);
                    }
                    if (HandCombination.getBestHand(new Hand(bestHandList[i].GetHand())) != HandCombination.getBestHand(new Hand(bestHandList[i + 1].GetHand())))
                        break;
                }
            }
            MainPot.Amount /= winners.Count;
            if (winners.Count > 1)
            {
                for (int i = 0; i < this.GetPlayers().Count; i++)
                {
                    if (winners.Contains(i))
                    {
                        _currentIndex = i;
                        activePlayers[i].CollectMoney(MainPot);
                        Winnermessage += activePlayers[i].Name + ", ";
                    }
                }
                Winnermessage += Environment.NewLine + " split the pot.";
            }
            else
            {
                _currentIndex = winners[0];
                activePlayers[_currentIndex].CollectMoney(MainPot);
                Winnermessage = activePlayers[_currentIndex].Message;
            }
            // CLIENTSIDE - FRONTEND; ON MATCH COMPLETED
            for (int i = 0; i < this.GetPlayers().Count; i++)
            {
                if (winners.Contains(i))
                {
                    activePlayers[i].playerObject.SendChatMessage("Winner= " + activePlayers[i].Name);
                }
                else
                {
                    activePlayers[i].playerObject.SendChatMessage("Loser= " + activePlayers[i].Name);
                }
                activePlayers[i].playerObject.TriggerEvent("OnMatchCompleted", Winnermessage);
            }
            //awarding sidepots
            for (int i = 0; i < SidePots.Count; i++)
            {
                List<int> sidePotWinners = new List<int>();
                for (int x = 0; x < bestHandList.Count; x++)
                {
                    for (int j = 0; j < this.GetPlayers().Count; j++)
                        if (activePlayers[j] == bestHandList[x] && SidePots[i].getPlayersInPot().Contains(bestHandList[x]))
                        {
                            sidePotWinners.Add(j);
                        }
                    if (HandCombination.getBestHand(new Hand(bestHandList[x].GetHand())) != HandCombination.getBestHand(new Hand(bestHandList[x + 1].GetHand())) && sidePotWinners.Count != 0)
                        break;
                }
                SidePots[i].Amount /= sidePotWinners.Count;
                for (int j = 0; j < this.GetPlayers().Count; j++)
                {
                    if (sidePotWinners.Contains(j))
                    {
                        _currentIndex = j;
                        activePlayers[j].CollectMoney(SidePots[i]);
                    }
                }
            }
            activePlayers.Clear();
            IsActive = false;
        }
        //check if it is necessary to create sidepots
        private bool CreateSidePots()
        {
            for (int i = 0; i < MainPot.getPlayersInPot().Count() - 1; i++)
            {
                if (MainPot.getPlayersInPot()[i].AmountInPot != MainPot.getPlayersInPot()[i + 1].AmountInPot)
                    return true;
            }
            return false;
        }
        PlayerList QuickSortBestHand(PlayerList myPlayers)
        {
            PokerPlayer pivot;
            Random ran = new Random();

            if (myPlayers.Count() <= 1)
                return myPlayers;
            pivot = myPlayers[ran.Next(myPlayers.Count())];
            myPlayers.Remove(pivot);

            var less = new PlayerList();
            var greater = new PlayerList();
            // Assign values to less or greater list
            foreach (PokerPlayer player in myPlayers)
            {
                if (HandCombination.getBestHand(new Hand(player.GetHand())) > HandCombination.getBestHand(new Hand(pivot.GetHand())))
                {
                    greater.Add(player);
                }
                else if (HandCombination.getBestHand(new Hand(player.GetHand())) <= HandCombination.getBestHand(new Hand(pivot.GetHand())))
                {
                    less.Add(player);
                }
            }
            // Recurse for less and greaterlists
            var list = new PlayerList();
            list.AddRange(QuickSortBestHand(greater));
            list.Add(pivot);
            list.AddRange(QuickSortBestHand(less));
            return list;
        }
        //check if everyone has folded except the player
        public bool PlayerWon()
        {
            if (MainPot.getPlayersInPot().Count == 1)
            {
                foreach (PokerPlayer player in this)
                {
                    if (player.isbusted)
                        continue;
                    if (player.IsFolded())
                        return true;
                }
            }
            return false;
        }



        //support for "foreach" loops
        public IEnumerator<PokerPlayer> GetEnumerator()
        {
            return activePlayers.GetEnumerator();
        }
    }
}
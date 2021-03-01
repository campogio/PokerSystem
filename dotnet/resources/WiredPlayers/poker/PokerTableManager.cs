﻿using System;
using System.Collections.Generic;
using System.Linq;
using GTANetworkAPI;

namespace SouthValleyFive.Scripts.Poker
{
    public class PokerTableManager 
    {
        private readonly PlayerList _players = new PlayerList();
        private int _maxPlayers;
        private Deck _deck;
        private readonly Hand _tableHand = new Hand();
        private int _roundCounter;
        private readonly Pot _mainPot;
        private readonly List<Pot> _sidePots;
        private readonly Random _rand;
        private int _turnCount;
        public string Winnermessage;
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
        //various propeties
        public int TurnCount
        {
            get { return _turnCount; }
            set { _turnCount = value; }
        }
        public int SmallBlind
        {
            get { return _smallBlind.Amount; }
        }
        public int BigBlind
        {
            get{return _bigBlind.Amount;}
        }
        public int RoundCount
        {
            get { return _roundCounter; }
            set { _roundCounter = value; }
        }
        /// <summary>
        /// contructor to begin the game, dealer position (and big/small blind position) is randomly choosen
        /// blinds are set to &500/1000 initially 
        /// </summary>
        /// <param name="players"></param>

        public PokerTableManager(PlayerList players, int maxPlayers)
        {
            this._players = players;
            _maxPlayers = maxPlayers;
            _deck = new Deck();
            _rand = new Random();
            _mainPot = new Pot();
            _sidePots = new List<Pot>();
            _smallBlind = new Blind();
            _bigBlind = new Blind();
            _roundCounter = 0;
            _turnCount = 0;
            _dealerPosition = _rand.Next(players.Count);
            //set blind amount and position
            _smallBlind.Amount = 500;
            _bigBlind.Amount = 1000;
            _mainPot.SmallBlind = 500;
            _mainPot.BigBlind = 1000;
            _smallBlind.Position = _dealerPosition + 1;
            _bigBlind.Position = _dealerPosition + 2;
            _currentIndex = _dealerPosition;
        }
        public PokerTableManager(int maxPlayers)
        {
            _maxPlayers = maxPlayers;
            _players = new PlayerList();
            _deck = new Deck();
            _rand = new Random();
            _mainPot = new Pot();
            _sidePots = new List<Pot>();
            _smallBlind = new Blind();
            _bigBlind = new Blind();
            _roundCounter = 0;
            _turnCount = 0;
            _dealerPosition = _rand.Next(_players.Count);
            //set blind amount and position
            _smallBlind.Amount = 500;
            _bigBlind.Amount = 1000;
            _mainPot.SmallBlind = 500;
            _mainPot.BigBlind = 1000;
            _smallBlind.Position = _dealerPosition + 1;
            _bigBlind.Position = _dealerPosition + 2;
            _currentIndex = _dealerPosition;
        }
        //indexer of players
        public PokerPlayer this[int index]
        {
            get
            {
                return _players.GetPlayer(ref index);
            }
            set
            {
                _players[index] = value;
            }
        }

        //various getters/setters
        public PlayerList GetPlayers()
        {
            return _players;
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
            return _mainPot;
        }
        public List<Pot> GetSidePots()
        {
            return _sidePots;
        }
        public Hand GetCommunityCards()
        {
            return _tableHand;
        }
        public Deck GetDeck()
        {
            return _deck;
        }
        /// <summary>
        /// Remove a player when the player busts out.
        /// </summary>
        /// <param name="player"></param>
        public void RemovePlayer(PokerPlayer player)
        {
            if (player.ChipStack != 0)
                throw new InvalidOperationException();
            _players.Remove(player);
        }
        public void RemovePlayer(int index)
        {
            if (_players[index].ChipStack != 0)
                throw new InvalidOperationException();
            _players.RemoveAt(index);
        }
        [RemoteEvent("JoinPokerMatch")]
        public void JoinMatch(Player player, int amount)
        {
            NAPI.Task.Run(() =>
            {
                if (_players.Count > _maxPlayers)
                {
                    player.TriggerEvent("showPokerErrorMessage", 0);
                    return;
                }
                
                _players.Add(new PokerPlayer(player.Name, amount, player));
                if (_players.Count >= 2)
                {
                player.TriggerEvent("JoinTable", "{fiches: amount, pot: _mainPot.Amount, playerName: player.Name, tableCards: _tableHand}");
                }
                
            });
        }
        [RemoteEvent("PokerLeaveEvent")]
        public void LeaveMatch(Player player)
        {
            
            if (_players.Count >= 2)
            {
                StartNextMatch();
            }
        }
        [RemoteEvent("PokerCallEvent")]
        public void PlayerCall(Player player) {
            // Need to check if it's their turn.
            PokerPlayer playerPoker = _players.First(p => p.Name == player.Name);
            playerPoker.Call(_mainPot);
            // CLIENTSIDE -> FRONTEND INTERFACE
            player.TriggerEvent("OnPlayerRaiseUpdated", "{minRaise: _mainPot.MinimumRaise, maxRaise: _mainPot.getMaximumAmountPutIn()}");
            player.TriggerEvent("OnPlayerPlayed", "{updatedPot: _mainPot.Amount, action: 'Call'}");
            //Browser.ExecuteJsFunction("OnPlayerRaiseUpdated({minRaise:"+ _mainPot.MinimumRaise +", maxRaise:"+ _mainPot.getMaximumAmountPutIn() +"});");
            //Browser.ExecuteJsFunction("OnPlayerPlayed({updatedPot:"+ _mainPot.Amount +", action:'Call'});");
        }
        [RemoteEvent("PokerRaiseEvent")]
        public void PlayerRaise(Player player, int fiches) {
            // Need to check if it's their turn.
            PokerPlayer playerPoker = _players.First(p => p.Name == player.Name);
            playerPoker.Raise(fiches, _mainPot);
            // CLIENTSIDE -> FRONTEND INTERFACE
            player.TriggerEvent("OnPlayerRaiseUpdated", "{minRaise: _mainPot.MinimumRaise, maxRaise: _mainPot.getMaximumAmountPutIn()}");
            player.TriggerEvent("OnPlayerPlayed", "{updatedPot: _mainPot.Amount, action: 'Raise'}");
        }
        /*mp.events.add({
            "PokerRaiseEvent": fiches => {
                PlayerRaise(fiches)
            },
        })*/
        [RemoteEvent("PokerFoldEvent")]
        public void PlayerFold(Player player, PokerPlayer pokerplayer) {
            // Need to check if it's their turn.
            pokerplayer.Fold(_mainPot);

            // CLIENTSIDE -> FRONTEND INTERFACE
            player.TriggerEvent("OnPlayerRaiseUpdated", "{minRaise: _mainPot.MinimumRaise, maxRaise: _mainPot.getMaximumAmountPutIn()}");
            player.TriggerEvent("OnPlayerPlayed", "{updatedPot: _mainPot.Amount, action: 'Fold'}");
        }
        
        /// <summary>
        /// Start a new round, dealer/smallblind position are moved up one spot
        /// players/counter variables are reset
        /// blinds are reset if necessary.
        /// </summary>
        public void StartNextMatch()
        {
            _players.ResetPlayers();
            _deck = new Deck();
            if (_roundCounter == 10)
            {
                _roundCounter = 0;
                _smallBlind.Amount *= 2;
                _bigBlind.Amount = _smallBlind.Amount * 2;
                _mainPot.SmallBlind = SmallBlind;
                _mainPot.BigBlind = BigBlind;
            }
            if (_roundCounter != 0)
            {
                _dealerPosition = IncrementIndex(_dealerPosition);
                _smallBlind.Position = IncrementIndex(_dealerPosition);
                _bigBlind.Position = IncrementIndex(_smallBlind.Position);
            }
            _roundCounter++;
            _mainPot.Amount = 0;
            _mainPot.AgressorIndex = -1;
            _mainPot.MinimumRaise = _bigBlind.Amount;
            _tableHand.Clear();
            _currentIndex = _dealerPosition;
            Winnermessage = null;
            _mainPot.getPlayersInPot().Clear();
            _sidePots.Clear();
            foreach (PokerPlayer pokerPlayer in _players)
            {
                ////Browser.ExecuteJsFunction($"StartNextMatch();");
                pokerPlayer.playerObject.TriggerEvent("showPokerMessage", "La partita sta per cominciare...");
                pokerPlayer.playerObject.TriggerEvent("StartNextMatch");
            }
        }


        /// <summary>
        /// Determine when the current betting round is over
        /// </summary>
        /// <returns></returns>
        public bool BeginNextTurn()
        {
            _turnCount++;
            while (_players[_mainPot.AgressorIndex].IsFolded()&&_currentIndex!=_mainPot.AgressorIndex)
                _mainPot.AgressorIndex = DecrementIndex(_mainPot.AgressorIndex);
            if (_currentIndex == _mainPot.AgressorIndex && _turnCount > 1)
                return false;
            else if (EveryoneAllIn())
                return false;
            else
                return true;
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
            if (zeroCount != 0 && totalCount==zeroCount)
                return true;
            else if (totalCount - zeroCount == 1)
            {
                for (int i = 0; i < GetPlayers().Count; i++)
                {
                    if (this[i].isbusted || this[i].IsFolded())
                        continue;
                    if (this[i].ChipStack != 0 && this[i].GetAmountToCall(_mainPot) == 0)
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
            while (_players.GetPlayer(ref currentIndex).IsFolded()||_players.GetPlayer(ref currentIndex).isbusted||_players.GetPlayer(ref currentIndex).ChipStack==0)
                currentIndex++;
            Player player = _players.GetPlayer(ref currentIndex).playerObject;
            // CLIENTSIDE -> FRONTEND
            int callValue = _players.GetPlayer(ref currentIndex).GetAmountToCall(_mainPot);
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
            while (_players.GetPlayer(ref currentIndex).IsFolded() || _players.GetPlayer(ref currentIndex).isbusted)
                currentIndex++;
            _players.GetPlayer(ref currentIndex);
            return currentIndex;
        }
        //same as increment class except in the other direction
        public int DecrementIndex(int currentIndex)
        {
            currentIndex--;
            while (_players.GetPlayer(ref currentIndex).IsFolded() || _players.GetPlayer(ref currentIndex).isbusted || _players.GetPlayer(ref currentIndex).ChipStack == 0)
                currentIndex--;
            _players.GetPlayer(ref currentIndex);
            return currentIndex;
        }

        //deal two unique cards to all players
        public void DealHoleCards(Player player)
        {
            _deck.Shuffle();
            for (int i = 0; i < _players.Count; i++)
            {
                if (i == 0)
                {
                    _players[i].AddToHand(_deck.Deal());
                    _players[i].AddToHand(_deck.Deal());
                }
                else
                {
                    _players[i].AddToHand(_deck.Deal(false));
                    _players[i].AddToHand(_deck.Deal(false));
                }
                ////Browser.ExecuteJsFunction($"GiveCards('{_players[i].getHand()}');");
                player.TriggerEvent("GiveCards", "{hand: _players[i].getHand()}");
            }
        }
        //pay small/big blind amount
        public void PaySmallBlind()
        {
            _players.GetPlayer(ref _smallBlind.Position).PaySmallBlind(_smallBlind.Amount, _mainPot,_currentIndex);
            _currentIndex = _smallBlind.Position;
        }
        public void PayBigBlind()
        {
            _players.GetPlayer(ref _bigBlind.Position).PayBigBlind(_bigBlind.Amount, _mainPot, _currentIndex);
            _currentIndex = _bigBlind.Position;
            _turnCount = 0;
        }
        //deal the flop
        public void DealFlop(Player player)
        {
            _tableHand.Add(_deck.Deal());
            _tableHand.Add(_deck.Deal());
            _tableHand.Add(_deck.Deal());
            for (int i = 0; i < _players.Count; i++)
            {
                _players[i].AddToHand(_tableHand);
            }
            // CLIENTSIDE -> FRONTEND
            /*Hand _deliverTableHand = new Hand();
            _deliverTableHand.Add(_tableHand[_tableHand.Count-3]);
            _deliverTableHand.Add(_tableHand[_tableHand.Count-2]);
            _deliverTableHand.Add(_tableHand[_tableHand.Count-1]);*/
            ////Browser.ExecuteJsFunction($"AddTableCard(" + _tableHand[_tableHand.Count()-3] + ");");
            ////Browser.ExecuteJsFunction($"AddTableCard(" + _tableHand[_tableHand.Count()-2] + ");");
            ////Browser.ExecuteJsFunction($"AddTableCard(" + _tableHand[_tableHand.Count()-1] + ");");
            player.TriggerEvent("AddTableCard", "{hand: _tableHand[_tableHand.Count()-3]}");
            player.TriggerEvent("AddTableCard", "{hand: _tableHand[_tableHand.Count()-2]}");
            player.TriggerEvent("AddTableCard", "{hand: _tableHand[_tableHand.Count()-1]}");
        }
        //deal the turn
        public void DealTurn(Player player)
        {
            Card turn = _deck.Deal();
            _tableHand.Add(turn);
            for (int i = 0; i < _players.Count; i++)
            {
                _players[i].AddToHand(turn);
            }
            // CLIENTSIDE -> FRONTEND
            ////Browser.ExecuteJsFunction($"AddTableCard(" + _tableHand[_tableHand.Count()-1] + ");");
            player.TriggerEvent("AddTableCard", "{hand: _tableHand[_tableHand.Count()-1]}");
        }
        //deal the river
        public void DealRiver(Player player)
        {
            Card river = _deck.Deal();
            _tableHand.Add(river);
            for (int i = 0; i < _players.Count; i++)
            {
                _players[i].AddToHand(river);
            }
            // CLIENTSIDE -> FRONTEND
            ////Browser.ExecuteJsFunction($"AddTableCard(" + _tableHand[_tableHand.Count()-1] + ");");
            player.TriggerEvent("AddTableCard", "{hand: _tableHand[_tableHand.Count()-1]}");
        }
        //showdown code!
        public void ShowDown()
        {
            //creating sidepots
            if (CreateSidePots())
            {
                _mainPot.getPlayersInPot().Sort();
                
                for (int i = 0; i < _mainPot.getPlayersInPot().Count - 1; i++)
                {
                    if (_mainPot.getPlayersInPot()[i].AmountInPot != _mainPot.getPlayersInPot()[i + 1].AmountInPot)
                    {
                        PlayerList tempPlayers = new PlayerList();
                        for (int j = _mainPot.getPlayersInPot().Count - 1; j > i; j--)
                        {
                            tempPlayers.Add(_mainPot.getPlayersInPot()[j]);
                        }
                        int potSize = (_mainPot.getPlayersInPot()[i + 1].AmountInPot - _mainPot.getPlayersInPot()[i].AmountInPot) * tempPlayers.Count;
                        _mainPot.Amount -= potSize;
                        _sidePots.Add(new Pot(potSize, tempPlayers));
                    }
                }
            }
            //awarding mainpot
            PlayerList bestHandList = new PlayerList();
            List<int> winners = new List<int>();
            bestHandList = QuickSortBestHand(new PlayerList(_mainPot.getPlayersInPot()));
            for (int i = 0; i < bestHandList.Count; i++)
            {
                for (int j = 0; j < this.GetPlayers().Count; j++)
                {
                    if (_players[j] == bestHandList[i])
                    {
                        winners.Add(j);
                    }
                    if (HandCombination.getBestHand(new Hand(bestHandList[i].GetHand())) != HandCombination.getBestHand(new Hand(bestHandList[i + 1].GetHand())))
                        break;
                }
            }
            _mainPot.Amount /= winners.Count;
            if (winners.Count > 1)
            {
                for (int i = 0; i < this.GetPlayers().Count; i++)
                {
                    if (winners.Contains(i))
                    {
                        _currentIndex = i;
                        _players[i].CollectMoney(_mainPot);
                        Winnermessage += _players[i].Name + ", ";
                    }
                }
                Winnermessage +=Environment.NewLine+ " split the pot.";
            }
            else
            {
                _currentIndex = winners[0];
                _players[_currentIndex].CollectMoney(_mainPot);
                Winnermessage = _players[_currentIndex].Message;
            }
            // CLIENTSIDE - FRONTEND; ON MATCH COMPLETED
            for (int i = 0; i < this.GetPlayers().Count; i++)
            {
                _players[i].playerObject.TriggerEvent("OnMatchCompleted", Winnermessage);
            }
            //awarding sidepots
            for (int i = 0; i < _sidePots.Count; i++)
            {
                List<int> sidePotWinners = new List<int>();
                for (int x = 0; x < bestHandList.Count; x++)
                {
                    for (int j = 0; j < this.GetPlayers().Count; j++)
                        if (_players[j] == bestHandList[x]&&_sidePots[i].getPlayersInPot().Contains(bestHandList[x]))
                        {
                            sidePotWinners.Add(j);
                        }
                    if (HandCombination.getBestHand(new Hand(bestHandList[x].GetHand())) != HandCombination.getBestHand(new Hand(bestHandList[x + 1].GetHand()))&&sidePotWinners.Count!=0)
                        break;
                }
                _sidePots[i].Amount /= sidePotWinners.Count;
                for (int j = 0; j < this.GetPlayers().Count; j++)
                {
                    if (sidePotWinners.Contains(j))
                    {
                        _currentIndex = j;
                        _players[j].CollectMoney(_sidePots[i]);
                    }
                }
            }
        }
        //check if it is necessary to create sidepots
        private bool CreateSidePots()
        {
            for(int i=0;i<_mainPot.getPlayersInPot().Count()-1;i++)
            {
                if (_mainPot.getPlayersInPot()[i].AmountInPot != _mainPot.getPlayersInPot()[i + 1].AmountInPot)
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
            if (_mainPot.getPlayersInPot().Count == 1)
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
            return _players.GetEnumerator();
        }
    }
}
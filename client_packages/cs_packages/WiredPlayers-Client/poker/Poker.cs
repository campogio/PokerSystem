using RAGE;
using WiredPlayers_Client.globals;
using System;
using RAGE.Ui;

namespace SouthValleyFiveClient.poker
{
    public class Poker : Events.Script
    {
        public Poker()
        {
            /* client -> server */
            Events.Add("JoinPokerMatch", JoinMatchEvent);
            Events.Add("PokerRaiseEvent", RaiseBetEvent);
            Events.Add("PokerCallEvent", CallBetEvent);
            Events.Add("PokerFoldEvent", FoldEvent);
            Events.Add("PokerLeaveEvent", LeaveMatchEvent);

            /* server -> client */
            Events.Add("JoinTable",JoinTable);
            Events.Add("OnPlayerRaiseUpdated",OnPlayerRaiseUpdated);
            Events.Add("OnPlayerPlayed",OnPlayerPlayed);
            Events.Add("showPokerMessage",ShowPokerMessage);
            Events.Add("showPokerErrorMessage",ShowPokerErrorMessage);
            Events.Add("StartNextMatch",StartNextMatch);
            Events.Add("OnPlayerTurn",OnPlayerTurn);
            Events.Add("ShowCards",ShowCards);
            Events.Add("GiveCards",GiveCards);
            Events.Add("AddTableCard",AddTableCard);
            Events.Add("OnMatchCompleted",OnMatchCompleted);
        }

        private void JoinMatchEvent(object[] args)
        {
            Events.CallRemote("JoinPokerMatch");
        }
        private void RaiseBetEvent(object[] args)
        {
            Events.CallRemote("PokerRaiseEvent", args);
        }
        private void CallBetEvent(object[] args)
        {
            Events.CallRemote("PokerCallEvent");
        }
        private void FoldEvent(object[] args) {
            Events.CallRemote("PokerFoldEvent");
        }
        private void LeaveMatchEvent(object[] args)
        {
            Events.CallRemote("pokerLeave");
            Events.CallLocal("destroyBrowser");
        }

        //private void JoinTable(int fiches, int pot, String playerName, int[] tableCards) {
        private void JoinTable(object[] args)
        {

            //Browser.ExecuteJsFunction("JoinTable(" + fiches + ',' + pot + ',' + playerName + ',' + tableCards + ")");
            Browser.CreateBrowser("pokerSystem.html", "destroyBrowser", null);
            Browser.ExecuteJsFunction("JoinTable(\"{\"fiches\":" + args[0] + ",\"pot\":" + args[1] + ",\"playerName\":" + args[2] + ",\"tableCards\":" + args[4] + "}\")");
        }
        //private void OnPlayerRaiseUpdated(int minRaise, int maxRaise) {
        private void OnPlayerRaiseUpdated(object[] args) {
            Browser.ExecuteJsFunction("OnPlayerRaiseUpdated(\"{\"minRaise\":" + args[0] + ",\"maxRaise\":" + args[1] + "}\")");
        }
        private void OnPlayerPlayed(object[] args) {
            Browser.ExecuteJsFunction("OnPlayerPlayed(\"{\"updatedPot\":" + args[0] + ",\"action\": + " + args[1] + "}\")");
        }
        private void ShowPokerMessage(object[] args) {
            //Browser.ExecuteJsFunction("showPokerMessage(" + arg + ")");
            Browser.ExecuteJsFunction("console.log(" + args[0] + ")");
        }
        private void ShowPokerErrorMessage(object[] args) {
            //Browser.ExecuteJsFunction("showPokerErrorMessage(" + arg + ")");
            Browser.ExecuteJsFunction("console.log(" + args[0] + ")");
        }
        private void StartNextMatch(object[] args) {
            Browser.ExecuteJsFunction("StartNextMatch()");
        }
        private void OnPlayerTurn(object[] args) {
            Browser.ExecuteJsFunction("OnPlayerTurn(\"{\"call\":" + args[0] + "}\")");
        }
        private void ShowCards(object[] args) {
            Browser.ExecuteJsFunction("ShowCards()");
        }
        private void GiveCards(object[] args) {
            Browser.ExecuteJsFunction("GiveCards(\"{\"hand\":" + args[0] + "}\")");
        }
        private void AddTableCard(object[] args) {
            Browser.ExecuteJsFunction("AddTableCard(\"{\"card\":" + args[0] + ",\"seed\":" + args[1] + "}\")");
        }
        private void OnMatchCompleted(object[] args) {
            Browser.ExecuteJsFunction("OnMatchCompleted(\"{\"winnerName\":" + args[0] + "}\")");
        }
    }
}
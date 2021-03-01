  
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// custom made list to support circular structure
/// all indexes over the count of list are wrapped around
/// ex. if the count is 5, and index of 5 is passed, the index will be converted into 0
/// </summary>
public class PlayerList : IList<PokerPlayer>
{
    List<PokerPlayer> list = new List<PokerPlayer>();

    public PlayerList()
    {
    }

    public PlayerList(PlayerList PlayerList)
    {
        this.list.AddRange(PlayerList.list);
    }

    public int IndexOf(PokerPlayer item)
    {
        return list.IndexOf(item);
    }

    public void Insert(int index, PokerPlayer item)
    {
        list.Insert(index, item);
    }

    public void RemoveAt(int index)
    {
        list.RemoveAt(index);
    }

    public PokerPlayer this[int index]
    {
        //wrap index around
        get
        {
            while (index > list.Count() - 1)
                index -= list.Count();
            while (index < 0)
                index += list.Count();
            return list[index];
        }
        set
        {
            while (index > list.Count() - 1)
                index -= list.Count();
            while (index < 0)
                index += list.Count();
            list[index] = value;
        }
    }

    public PokerPlayer GetPlayer(ref int index)
    {
        //wrap index and changing the index passed through
        while (index > list.Count() - 1)
            index -= list.Count();
        while (index < 0)
            index += list.Count();
        return list[index];
    }

    public void Add(PokerPlayer item)
    {
        list.Add(item);
    }

    public void AddRange(PlayerList players)
    {
        list.AddRange(players);
    }

    public void Clear()
    {
        list.Clear();
    }

    public bool Contains(PokerPlayer item)
    {
        return list.Contains(item);
    }

    public void CopyTo(PokerPlayer[] array, int arrayIndex)
    {
        list.CopyTo(array, arrayIndex);
    }

    public int Count
    {
        get { return list.Count; }
    }

    public bool IsReadOnly
    {
        get { return false; }
    }

    public bool Remove(PokerPlayer item)
    {
        return list.Remove(item);
    }

    public IEnumerator<PokerPlayer> GetEnumerator()
    {
        return list.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return list.GetEnumerator();
    }

    //reset all players in list
    public void ResetPlayers()
    {
        foreach (PokerPlayer player in this)
            player.Reset();
    }

    public void Sort()
    {
        list = QuickSortPlayers(list);
    }

    List<PokerPlayer> QuickSortPlayers(List<PokerPlayer> myPlayers)
    {
        PokerPlayer pivot;
        Random ran = new Random();

        if (myPlayers.Count() <= 1)
            return myPlayers;
        pivot = myPlayers[ran.Next(myPlayers.Count())];
        myPlayers.Remove(pivot);

        var less = new List<PokerPlayer>();
        var greater = new List<PokerPlayer>();
        // Assign values to less or greater list
        for (int i = 0; i < myPlayers.Count(); i++)
        {
            if (myPlayers[i].AmountInPot > pivot.AmountInPot)
            {
                greater.Add(myPlayers[i]);
            }
            else if (myPlayers[i].AmountInPot <= pivot.AmountInPot)
            {
                less.Add(myPlayers[i]);
            }
        }

        // Recurse for less and greaterlists
        var list = new List<PokerPlayer>();
        list.AddRange(QuickSortPlayers(less));
        list.Add(pivot);
        list.AddRange(QuickSortPlayers(greater));


        return list;
    }
}
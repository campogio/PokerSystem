﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Resources;
public enum RANK
{
    TWO=2, THREE, FOUR, FIVE, SIX, SEVEN, EIGHT, NINE, TEN, JACK, QUEEN, KING, ACE
}
public enum SUIT
{
    DIAMONDS = 1,
    CLUBS,
    HEARTS,
    SPADES
}
/// <summary>
/// THIS IS THE CLASS THAT STARTED EVERYTHING AND MADE THIS GAME POSSIBLE
/// </summary>
public class Card
{
    protected bool Equals(Card other)
    {
        return rank == other.rank && suit == other.suit && faceUp == other.faceUp && highlight == other.highlight;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((Card) obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(rank, suit, faceUp, highlight);
    }

    private int rank, suit;
    private bool faceUp;
    private bool highlight;
    public bool FaceUp
    {
        get { return faceUp; }
        set 
        { 
            faceUp = value;
        }
    }
    //default two of diamonds
    public Card()
    {
        rank = (int)RANK.TWO;
        suit = (int)SUIT.DIAMONDS;
        faceUp = false;
        highlight = false;
    }
    public Card(RANK rank,SUIT suit)
    {
        this.rank = (int)rank;
        this.suit = (int)suit;
        faceUp = false;
        highlight = false;
    }
    public Card(int rank, int suit)
    {
        if (rank < 1 || rank > 14 || suit < 1 || suit > 4)
            throw new ArgumentOutOfRangeException();
        this.rank=rank;
        this.suit=suit;
        faceUp=false;
        highlight = false;
    }
    public Card(RANK rank, SUIT suit,bool faceUp)
    {
        this.rank = (int)rank;
        this.suit = (int)suit;
        this.faceUp = faceUp;
        highlight = false;
    }
    public Card(int rank, int suit,bool faceUp)
    {
        if (rank < 1 || rank > 14 || suit < 1 || suit > 4)
            throw new ArgumentOutOfRangeException();
        this.rank = rank;
        this.suit = suit;
        this.faceUp = faceUp;
        highlight = false;
    }
    public Card(Card card)
    {
        this.rank = card.rank;
        this.suit = card.suit;
        this.faceUp = card.faceUp;
        highlight = false;
    }
    public static string rankToString(int rank)
    {
        switch (rank)
        {
            case 11:
                return "Jack";
            case 12:
                return "Queen";
            case 13:
                return "King";
            case 14:
                return "Ace";
            default:
                return rank.ToString();
        }
    }
    public static string suitToString(int suit)
    {
        switch (suit)
        {
            case 1:
                return "Diamonds";
            case 2:
                return "Clubs";
            case 3:
                return "Hearts";
            default:
                return "Spades";
        }
    }
    public int getRank()
    {
        return rank;
    }
    public int getSuit()
    {
        return suit;
    }
    public void setRank(RANK rank)
    {
        this.rank = (int)rank;
    }
    public void setCard(RANK rank, SUIT suit)
    {
        this.rank = (int)rank;
        this.suit = (int)suit;
    }
    public void setCard(int rank, int suit)
    {
        if(rank<1||rank>14||suit<1||suit>4)
            throw new ArgumentOutOfRangeException();
        this.rank=rank;
        this.suit=suit;
    }
    public override string ToString()
    {
        if (faceUp == true)
            return rankToString(rank) + " of " + suitToString(suit);
        return "The card is facedown, you cannot see it!";
    }
    public bool isHighlighted()
    {
        return this.highlight;
    }

    //compare rank of cards
    public static bool operator ==(Card a, Card b)
    {
        if (a.rank == b.rank)
            return true;
        else
            return false;
    }
    public static bool operator !=(Card a, Card b)
    {
        if (a.rank != b.rank)
            return true;
        else
            return false;
    }
    public static bool operator <(Card a, Card b)
    {
        if (a.rank < b.rank)
            return true;
        else
            return false;
    }
    public static bool operator >(Card a, Card b)
    {
        if (a.rank > b.rank)
            return true;
        else
            return false;
    }
    public static bool operator <=(Card a, Card b)
    {
        if (a.rank <= b.rank)
            return true;
        else
            return false;
    }
    public static bool operator >=(Card a, Card b)
    {
        if (a.rank >= b.rank)
            return true;
        else
            return false;
    }
}
    
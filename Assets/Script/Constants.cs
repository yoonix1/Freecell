
public class Constants
{
    public const int NUMBER_OF_CARDS = 52;
    public static float CARD_WIDTH = 55;
    public static float CARD_HEIGHT = 75;

    public static float CARD_GAP = 15;
    public static float CARD_PADDING = 10;
    public static float PILE_GAP = 10;
    public static float PILE_OFFSET = 30;
    
    public static float SCREEN_OFFSET_H = 375;
    public static float SCREEN_OFFSET_W = -245;
    public static float SCREEN_OFFSET_PILE = 50;

    public static float SCREEN_CARDS_OFFSET_H = SCREEN_OFFSET_H - CARD_HEIGHT - 15;


}

public enum Suit
{
    HEART = 0,
    SPADE = 1,
    DIAMOND = 2,
    CLUB = 3,
    COUNT = 4
}

public enum DropZoneMode
{
    Work,
    Pile, 
    Deck,
}


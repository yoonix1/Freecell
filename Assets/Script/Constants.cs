
public class Constants
{
    public const int NUMBER_OF_CARDS = 52;
    public static float CARD_WIDTH = 66;
    public static float CARD_HEIGHT = 100;
    
    public static float CARD_PADDING_H = 50;
    public static float CARD_PADDING_W = 80;
    
    public static float SCREEN_OFFSET_H = -250;
    public static float SCREEN_OFFSET_W = 80;
}

public enum Suit
{
    HEART,
    SPADE,
    DIAMOND,
    CLUBS,
    COUNT
}

public enum DropZoneMode
{
    Work,
    Pile, 
    Deck,
}

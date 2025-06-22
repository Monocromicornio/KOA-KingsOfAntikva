public interface InterfaceSquad
{
    TableData table { get; set; }
    BoardController board => BoardController.instance;
    Field[] fields => board.fields.ToArray();
}
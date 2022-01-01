namespace ApDownloader.Model;

public class Cell
{
    public Cell(int productId, string imageUrl, string name, bool canUpdate = false, bool isNotOnDisk = false)
    {
        ProductId = productId;
        ImageUrl = imageUrl;
        Name = name;
        CanUpdate = canUpdate;
        IsNotOnDisk = isNotOnDisk;
    }


    public int ProductId { get; set; }
    public string ImageUrl { get; set; }
    public string Name { get; set; }
    public bool CanUpdate { get; set; }
    public bool IsNotOnDisk { get; set; }

    public bool Equals(string other)
    {
        return Name == other;
    }

    public override bool Equals(object? obj)
    {
        if (obj == null) return false;
        var cellObj = obj as Cell;
        return cellObj != null && Equals(cellObj.Name, Name);
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }

    public static bool operator ==(Cell cell1, Cell cell2)
    {
        return cell1.Equals(cell2);
    }

    public static bool operator !=(Cell? cell1, Cell cell2)
    {
        return !cell1.Equals(cell2);
    }
}
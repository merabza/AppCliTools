//using System;
//using CliParameters;
//using CliToolsData.Models;
//using SystemToolsShared;

//namespace CliToolsData.Domain;

//public sealed class FileStorageDataDomain : ItemData
//{

//    public string FileStoragePath { get; }
//    public string? UserName { get; }
//    public string? Password { get; }
//    public int FileNameMaxLength { get; }
//    public int FileSizeSplitPositionInRow { get; }

//   public FileStorageDataDomain(string fileStoragePath, string? userName, string? password, int fileNameMaxLength, int fileSizeSplitPositionInRow)
//    {
//        FileStoragePath = fileStoragePath;
//        UserName = userName;
//        Password = password;
//        FileNameMaxLength = fileNameMaxLength;
//        FileSizeSplitPositionInRow = fileSizeSplitPositionInRow;
//    }

//    //public bool IsSameToLocal(string localPath)
//    //{
//    //  //ან თუ წყაროს ფაილსაცავი ლოკალურია და მისი ფოლდერი ემთხვევა პარამეტრების ლოკალურ ფოლდერს.
//    //  //   მაშინ მოქაჩვა საჭირო აღარ არის
//    //  if (!FileStat.IsFileSchema(FileStoragePath))
//    //    return true;

//    //  return FileStat.NormalizePath(localPath) != FileStat.NormalizePath(FileStoragePath);
//    //}

//    public bool IsFileSchema()
//    {
//        if (string.IsNullOrWhiteSpace(FileStoragePath))
//            throw new Exception("FileStoragePath is empty");
//        return FileStat.IsFileSchema(FileStoragePath);
//    }

//    public bool IsFtp()
//    {
//        if (!Uri.TryCreate(FileStoragePath, UriKind.Absolute, out var uri))
//            return false;
//        return uri.Scheme.ToLower() == "ftp";
//    }

//    public static bool IsSameToLocal(FileStorageData forFileStorage, string localPath)
//    {
//        //ან თუ წყაროს ფაილსაცავი ლოკალურია და მისი ფოლდერი ემთხვევა პარამეტრების ლოკალურ ფოლდერს.
//        //   მაშინ მოქაჩვა საჭირო აღარ არის
//        if (forFileStorage.FileStoragePath is null || string.IsNullOrWhiteSpace(localPath))
//        {
//            Console.WriteLine("IsSameToLocal Returns false because forFileStorage is null or localPath empty ");
//            return false;
//        }

//        if (FileStat.IsFileSchema(forFileStorage.FileStoragePath))
//            return FileStat.NormalizePath(localPath) == FileStat.NormalizePath(forFileStorage.FileStoragePath);

//        Console.WriteLine($"IsSameToLocal Returns true because {forFileStorage.FileStoragePath} is not file schema");
//        return false;
//    }


//}


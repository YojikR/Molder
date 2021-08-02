﻿using System;
using Molder.Exceptions;
using Molder.Helpers;
using Molder.Models.Directory;
using Molder.Models.Profider;
using Microsoft.Extensions.Logging;

namespace Molder.Models.File
{
    public class TextFile : IFile, IDisposable
    {
        public IDirectory UserDirectory { get; set; } = new UserDirectory();
        public string? Filename { get; set; }
        public string? Path { get; set; }
        public string? Content { get; set; }
        public string Url { get; set; }

        public IFileProvider FileProvider = new FileProvider();
        public IPathProvider PathProvider = new PathProvider();
        public IWebProvider WebProvider = new WebProvider();
        
        public bool IsExist(string filename, string path = null!)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                path = UserDirectory.Get();
            }

            var fullpath = PathProvider.Combine(path, filename);

            return FileProvider.Exist(fullpath);
        }

        public bool DownloadFile(string url, string filename, string pathToSave = null!)
        {
            if (string.IsNullOrWhiteSpace(pathToSave))
            {
                pathToSave = UserDirectory.Get();
            }

            if (string.IsNullOrWhiteSpace(filename))
            {
                Log.Logger().LogWarning("DOWNLOAD: FileName is missing");
                throw new ArgumentException("DOWNLOAD: FileName is missing");
            }
            else
            {
                bool isValidExtension = FileProvider.CheckFileExtension(filename);
                if (isValidExtension)
                {
                    if (WebProvider.Download(url, pathToSave, filename)) return true;
                    else return false;
                }
                else
                {
                    Log.Logger().LogWarning($"Check that the file \"{filename}\" has a .txt extension");
                    throw new ValidFileNameException($"Check that the file \"{filename}\" has a .txt extension");
                }
            }
        }

        public bool Create(string filename, string path = null!, string content = null!)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                path = UserDirectory.Get();
            }

            bool isNull = string.IsNullOrEmpty(filename);
            if (!isNull)
            {
                bool IsTxt = FileProvider.CheckFileExtension(filename);

                if (IsTxt)
                {
                    var exist = IsExist(filename, path);
                    if (!exist)
                    {
                        if (string.IsNullOrWhiteSpace(content))
                        {
                            return FileProvider.Create(filename, path, content);

                        }
                        else
                        {
                            return FileProvider.AppendAllText(filename, path, content);
                        }
                    }
                    else
                    {
                        return FileProvider.WriteAllText(filename, path, content);
                    }

                }
                else
                {
                    throw new FileExtensionException($"The file \"{filename}\" is not a text file");
                }
            }
            else
            {
                throw new NoFileNameException("CREATE: FileName is missing");
            }
        }

        public bool Delete(string filename, string path = null!)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                path = UserDirectory.Get();
            }

            if (string.IsNullOrWhiteSpace(filename))
            {
                Log.Logger().LogWarning("DELETE: FileName is missing");
                throw new NoFileNameException("DELETE: FileName is missing");
            }

            if (IsExist(filename, path))
            {
                var fullpath = PathProvider.Combine(path, filename);
                return FileProvider.Delete(fullpath);
            }
            else
            {
                Log.Logger().LogWarning($"The file \"{filename}\" does not exist in the \"{path}\" directory");
                throw new FileExistException($"The file \"{filename}\" does not exist in the \"{path}\" directory");
            }
        }

        public string? GetContent(string filename, string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                path = UserDirectory.Get();
            }
            
            if (string.IsNullOrWhiteSpace(filename))
            {
                Log.Logger().LogWarning("GET CONTENT: FileName is missing");
                throw new NoFileNameException("GET CONTENT: FileName is missing");
            }

            if (IsExist(filename, path))
            {
                return FileProvider.ReadAllText(filename, path);
            }

            Log.Logger().LogWarning($"The file \"{filename}\" does not exist in the \"{path}\" directory");
            throw new FileExistException($"The file \"{filename}\" does not exist in the \"{path}\" directory");
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}

﻿namespace MiniblogToGhost.Ghost
{
    using CloudinaryDotNet;
    using CloudinaryDotNet.Actions;

    using MiniblogToGhost.Miniblog;

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;

    public static class Utils
    {
        public static GhostFormat BuildGhostObject(List<post> postList, Options options)
        {
            var data = new GhostFormat
            {
                meta = BuildMeta(),
                data = new Data()
            };

            ParsePosts(data, postList, options);

            return data;
        }

        public static string ConvertHtmlToMarkdown(string html)
        {
            //var enviromentPath = System.Environment.GetEnvironmentVariable("PATH");
            //var paths = enviromentPath.Split(';');
            //var exePath = paths.Select(x => Path.Combine(x, "pandoc.exe"))
            //                   .Where(x => File.Exists(x))
            //                   .FirstOrDefault();

            //if (exePath == null)
            //{
            //    throw new Exception("Failed to find pandoc - have you installed it?");
            //}

            var args = string.Format(@"-r html -t markdown");

            var p = new Process
            {
                StartInfo = new ProcessStartInfo("pandoc.exe", args)
                {
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    UseShellExecute = false
                }
            };
            p.Start();

            var outputString = string.Empty;
            var inputBuffer = Encoding.UTF8.GetBytes(html);
            p.StandardInput.BaseStream.Write(inputBuffer, 0, inputBuffer.Length);
            p.StandardInput.Close();

            p.WaitForExit(2000);
            using (var sr = new StreamReader(p.StandardOutput.BaseStream))
            {
                outputString = sr.ReadToEnd();
            }

            return outputString;
        }

        public static string GenerateSlug(string phrase)
        {
            var str = phrase.RemoveAccent().ToLower();
            // invalid chars
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
            // convert multiple spaces into one space
            str = Regex.Replace(str, @"\s+", " ").Trim();
            // cut and trim
            str = str.Substring(0, str.Length <= 45 ? str.Length : 45).Trim();
            str = Regex.Replace(str, @"\s", "-"); // hyphens
            return str;
        }

        public static string RemoveAccent(this string txt)
        {
            var bytes = Encoding.GetEncoding("Cyrillic").GetBytes(txt);
            return Encoding.ASCII.GetString(bytes);
        }

        private static Meta BuildMeta() =>
            new Meta()
            {
                exported_on = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                version = "003"
            };

        private static int GetOrCreateTag(GhostFormat data, string category)
        {
            var existingTag = data.data.tags.FirstOrDefault(t => t.name.Equals(category, StringComparison.InvariantCultureIgnoreCase));

            if (existingTag is null)
            {
                // Create a new tag
                var tag = new Tag
                {
                    id = (data.data.tags.Count > 0) ? data.data.tags.Max(t => t.id) + 1 : 1,
                    name = category,
                    slug = GenerateSlug(category),
                    description = ""
                };

                data.data.tags.Add(tag);
                existingTag = tag;
            }

            return existingTag.id;
        }

        private static long GetUnixTime(string dateTime)
        {
            if (DateTimeOffset.TryParse(dateTime, out var parsedDateTime))
            {
                return parsedDateTime.ToUnixTimeMilliseconds();
            }
            else
            {
                throw new Exception("Failed To parse datetime");
            }
        }

        private static void ParsePosts(GhostFormat data, List<post> postList, Options options)
        {
            var id = 1;
            foreach (var post in postList.OrderBy(p => p.pubDate))
            {
                if (data.data.posts is null)
                {
                    data.data.posts = new List<Post>();
                }

                post.content = SanitiseImagesAndLinks(post.content, options);

                var ghostPost = new Post
                {
                    id = id,
                    title = post.title,
                    slug = post.slug,
                    html = post.content,
                    markdown = ConvertHtmlToMarkdown(post.content),
                    featured = 0,
                    page = 0,
                    status = post.ispublished ? "published" : "draft",
                    language = "en_GB",
                    meta_title = null,
                    meta_description = null,
                    author_id = 1,  // Default to 1
                    created_at = GetUnixTime(post.lastModified),
                    created_by = 1,  // Default to 1
                    updated_at = GetUnixTime(post.lastModified),
                    updated_by = 1,  // Default to 1
                    published_at = GetUnixTime(post.pubDate),
                    published_by = 1  // Default to 1
                };

                data.data.posts.Add(ghostPost);

                // Look at tags
                ProcessTags(data, id, post);

                id++;
            }
        }

        private static void ProcessTags(GhostFormat data, int postId, post post)
        {
            if (data.data.tags is null)
            {
                data.data.tags = new List<Tag>();
            }

            if (data.data.posts_tags is null)
            {
                data.data.posts_tags = new List<Posts_Tags>();
            }

            // Extract the categories
            foreach (var category in post.categories)
            {
                // find category in ghost data
                var tagId = GetOrCreateTag(data, category);

                // Create tag/post mapping

                if (data.data.posts_tags == null)
                {
                    data.data.posts_tags = new List<Posts_Tags>();
                }

                data.data.posts_tags.Add(new Posts_Tags()
                {
                    post_id = postId,
                    tag_id = tagId
                });
            }
        }

        private static string SanitiseImagesAndLinks(string content, Options options)
        {
            var cloudinaryAccount = new Account(options.CloudinaryCloudName, options.CloudinaryApiKey, options.CloudinaryApiSecret);
            var cloudinary = new Cloudinary(cloudinaryAccount);

            // Look for relative Urls
            var srcRegex = "(src=[\"'])(/posts/files/|https?://www.gregpakes.co.uk/posts/files/)(.+?)([\"'].*?)";
            //string hrefRegex = "(href=[\"'])(/posts/files/||http://www.gregpakes.co.uk/posts/files/)(.+?)([\"'].*?)";

            foreach (Match match in Regex.Matches(content, srcRegex))
            {
                // Loop through the matches Get the image and then upload to cloudinary
                var localPath = string.Format("files\\{0}", match.Groups[3].Value.Replace("/", "\\"));
                var fullPath = Path.Combine(options.InputDirectory, localPath);
                var decodedFullPath = HttpUtility.UrlDecode(fullPath);
                string finalPath = null;
                if (File.Exists(decodedFullPath))
                {
                    finalPath = decodedFullPath;
                }
                else
                {
                    if (File.Exists(fullPath))
                    {
                        finalPath = fullPath;
                    }
                    else
                    {
                        throw new Exception("Image not found");
                    }
                }

                if (finalPath != null)
                {
                    Logger.Log("Uploading {0} to cloudinary.", finalPath);
                    // Upload file to Cloudinary
                    var uploadParams = new ImageUploadParams()
                    {
                        File = new FileDescription(finalPath)
                    };
                    var uploadResult = cloudinary.Upload(uploadParams);
                    var replacementUri = uploadResult.SecureUrl;

                    // Should cover all urls
                    content = content.Replace(match.Groups[2].Value + match.Groups[3].Value, replacementUri.ToString());
                }
            }

            //content = Regex.Replace(content, hrefRegex, "$1/images/$3$4");

            return content;
        }

        //private static int GetOrCreateAuthor(GhostFormat data, string author)
        //{
        //    // Check if the author already exists
        //    if (data.data.users == null)
        //    {
        //        data.data.users = new List<User>();
        //    }

        // var existingUser = data.data.users.FirstOrDefault(u => u.name == author);

        //    if ()
        //}
    }
}

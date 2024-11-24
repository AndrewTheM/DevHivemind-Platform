import pandas as pd
import pyodbc

# Database connection parameters
connection_string = "DRIVER={SQL Server};SERVER=localhost,14330;DATABASE=BlogPlatform_Posts;UID=sa;PWD=1Q2w3e4r"

# Establish database connection
conn = pyodbc.connect(connection_string)

# Query data for "Medium Blog Data.csv"
query_blog_data = """
    SELECT 
        p.Id AS blog_id,
        p.AuthorId AS author_id,
        p.Title AS blog_title,
        pc.Content AS blog_content,
        CONCAT('http://localhost:8020/posts/', p.TitleIdentifier) AS blog_link,
        p.ThumbnailPath AS blog_img,
        t.TagName AS topic
    FROM 
        Posts p
    INNER JOIN 
        PostContents pc ON p.Id = pc.Id
    LEFT JOIN 
        TagsOfPosts tp ON p.Id = tp.PostId
    LEFT JOIN 
        Tags t ON tp.TagId = t.Id
"""
blog_data = pd.read_sql(query_blog_data, conn)

# Query data for "Blog Ratings.csv"
query_ratings_data = """
    SELECT 
        r.PostId AS blog_id,
        r.UserId AS userId,
        r.RatingValue AS ratings
    FROM 
        Ratings r
"""
ratings_data = pd.read_sql(query_ratings_data, conn)

# Save datasets to CSV files
blog_data.to_csv("db_blog_data.csv", index=False)
ratings_data.to_csv("db_blog_ratings.csv", index=False)

# Close the database connection
conn.close()
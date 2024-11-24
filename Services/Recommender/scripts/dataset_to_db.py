import re
import pandas as pd
import pyodbc
import uuid
import json
import os

# Function to generate a consistent UUID for each integer ID
def generate_uuid(id_mapping, original_id):
    if original_id not in id_mapping:
        id_mapping[original_id] = str(uuid.uuid4())
    return id_mapping[original_id]

# Functions to save and load mappings for IDs from dataset values to database uuids
def load_mappings(path):
    if os.path.exists(path):
        with open(path, 'r') as file:
            return json.load(file)
    return {}

def save_mappings(path, mappings):
    with open(path, 'w') as file:
        json.dump(mappings, file, indent=4)

# Database connection parameters
connection_string = 'DRIVER={SQL Server};SERVER=localhost,14330;DATABASE=BlogPlatform_Posts;UID=sa;PWD=1Q2w3e4r'
blog_id_map_path = 'data/blog_id_map.json'
user_id_map_path = 'data/user_id_map.json'
tag_id_map_path = 'data/tag_id_map.json'

# Reading CSV files
blog_data = pd.read_csv('data/blog_data.csv')
ratings_data = pd.read_csv('data/blog_interactions.csv')
authors_data = pd.read_csv('data/blog_authors.csv')

# Merging author names to blog data
blog_data = pd.merge(blog_data, authors_data, on='author_id', how='left')

# Dictionary to store mapping from integer ID to UUID
blog_uuid_mapping = load_mappings(blog_id_map_path)
user_uuid_mapping = load_mappings(user_id_map_path)
topic_to_id = load_mappings(tag_id_map_path)

# Establish database connection
conn = pyodbc.connect(connection_string)
cursor = conn.cursor()

counter = 0

# Process "Posts" and "PostContents" tables
for index, row in blog_data.iterrows():
    blog_id = str(row['blog_id'])
    blog_uuid = generate_uuid(blog_uuid_mapping, blog_id)
    author_uuid = generate_uuid(user_uuid_mapping, str(row['author_id']))
    author_name = str(row['author_name'])
    title = str(row['blog_title'])[:200]
    title_identifier = str(row['blog_link']).split('/')[-1].split('?')[0][:95] + blog_id
    # Get full size images for posts
    thumbnail_path = re.sub(r'\/resize:fill:\d{2,4}:\d{2,4}\/|\/fit\/c\/\d{2,4}\/\d{2,4}\/', '/', str(row['blog_img']))

    try:
        # Insert into Posts table
        cursor.execute("""
            INSERT INTO Posts (Id, AuthorId, Author, Title, TitleIdentifier, ThumbnailPath)
            VALUES (?, ?, ?, ?, ?, ?)
            """, blog_uuid, author_uuid, author_name, title, title_identifier, thumbnail_path)

        # Insert into PostContents table
        cursor.execute("""
            INSERT INTO PostContents (Id, Content)
            VALUES (?, ?)
            """, blog_uuid, str(row['blog_content']))
        
        counter += 1
    except:
        print(f'Faulty record: {row}')
    
    if counter % 100 == 0:
        print(f'Inserted {counter} Posts')

conn.commit()
print(f'Done with Posts, {counter} total')
counter = 0

save_mappings(blog_id_map_path, blog_uuid_mapping)
save_mappings(user_id_map_path, user_uuid_mapping)

# Process "Ratings" table
for index, row in ratings_data.iterrows():
    blog_uuid = generate_uuid(blog_uuid_mapping, str(row['blog_id']))
    user_uuid = generate_uuid(user_uuid_mapping, str(row['userId']))
    rating = float(row['ratings'])
    # Correcting rating values to match general user behavior
    match rating:
        case 0.5:
            rating = 3
        case 2:
            rating = 4
        case 3.5:
            rating = 4

    try:
        cursor.execute("""
            INSERT INTO Ratings (UserId, PostId, RatingValue)
            VALUES (?, ?, ?)
            """, user_uuid, blog_uuid, rating)
        counter += 1
    except:
        print(f'Faulty record: {row}')
    
    if counter % 1000 == 0:
        print(f'Inserted {counter} Ratings')

conn.commit()
print(f'Done with Ratings, {counter} total')
counter = 0

# Process "Tags" and "TagsOfPosts" tables
unique_topics = str(blog_data['topic']).unique()

# Insert unique tags into the Tags table and map them to their UUIDs
for topic in unique_topics:
    tag_uuid = str(uuid.uuid4())
    topic_to_id[topic] = tag_uuid
    
    try:
        cursor.execute("""
            INSERT INTO Tags (Id, TagName)
            VALUES (?, ?)
            """, tag_uuid, topic)
        counter += 1
    except:
        print(f'Faulty value: {topic}')
        
    if counter % 10 == 0:
        print(f'Inserted {counter} Tags')

conn.commit()
print(f'Done with Tags, {counter} total')
counter = 0

save_mappings(tag_id_map_path, topic_to_id)

# Map each post to its tags in the TagsOfPosts table
for index, row in blog_data.iterrows():
    blog_uuid = blog_uuid_mapping[str(row['blog_id'])]
    tag_uuid = topic_to_id[str(row['topic'])]

    try:
        cursor.execute("""
            INSERT INTO TagsOfPosts (TagsId, PostsId)
            VALUES (?, ?)
            """, tag_uuid, blog_uuid)
        counter += 1
    except:
        print(f'Faulty record: {row}')
        
    if counter % 100 == 0:
        print(f'Inserted {counter} Tags of Posts')


conn.commit()
print(f'Done with Tags of Posts, {counter} total')

# Commit the transaction and close the connection
cursor.close()
conn.close()
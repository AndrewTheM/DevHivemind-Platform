import json
import os
import torch
from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from model.gnn_recommender import GNNRecommender


GRAPH_DATA = 'output/graph_data.pt'
MODEL_STATE = 'output/gnn_recommender.pth'
BLOG_MAPPINGS = 'data/blog_id_map.json'
USER_MAPPINGS = 'data/user_id_map.json'
HIDDEN_DIM = 8

app = FastAPI()
app.add_middleware(
    CORSMiddleware,
    allow_origins=['http://localhost:8009', 'https://localhost:18009', 'http://aggregator'],
    allow_credentials=True,
    allow_methods=['GET'],
    allow_headers=['*'],
)


def recommend_blogs(model, data, user_id, top_k):
    model.eval()
    with torch.no_grad():
        all_blogs = torch.arange(data.x.size(0))
        
        # Blogs the user has interacted with
        interacted_blogs = data.edge_index[1, data.edge_index[0] == user_id].unique()
        
        # Blogs the user has not interacted with
        non_interacted_blogs = all_blogs[~torch.isin(all_blogs, interacted_blogs)]
        
        # Create user-blog edge index for prediction
        user_id_tensor = torch.tensor([user_id] * len(non_interacted_blogs), dtype=torch.long)
        edge_index = torch.stack((user_id_tensor, non_interacted_blogs.long()))

        # Predict interaction scores for non-interacted blogs
        predictions = model(data.x, edge_index, None).squeeze()
        
        # Get top-k blog recommendations
        top_k_pred = predictions.topk(k=min(top_k, len(non_interacted_blogs)), largest=True)
        recommended_blogs = non_interacted_blogs[top_k_pred.indices]
        
    return recommended_blogs.tolist()

def load_mappings(path):
    if os.path.exists(path):
        with open(path, 'r') as file:
            return json.load(file)
    return {}

def map_key_by_value(map, value):
    return next((k for k, v in map.items() if v == value), None)

@app.get('/recommend', response_model=list[str])
async def get_recommendations_for_user(user_id: str, top_k: int):
    graph_data = torch.load(GRAPH_DATA)
    model_state = torch.load(MODEL_STATE)
    blog_mappings = load_mappings(BLOG_MAPPINGS)
    user_mappings = load_mappings(USER_MAPPINGS)
    
    model = GNNRecommender(input_dim=graph_data.x.shape[1], hidden_dim=HIDDEN_DIM, output_dim=1)
    model.load_state_dict(model_state)
    
    user_id = map_key_by_value(user_mappings, user_id) or 0
    recommended_blogs = recommend_blogs(model, graph_data, user_id, top_k)
    
    blog_ids = [blog_mappings[str(id)] for id in recommended_blogs]
    return blog_ids

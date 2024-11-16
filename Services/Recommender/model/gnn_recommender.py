import torch.nn.functional as F
from torch.nn import Module
from torch_geometric.nn import GCNConv


class GNNRecommender(Module):
    def __init__(self, input_dim, hidden_dim, output_dim):
        super(GNNRecommender, self).__init__()
        self.conv1 = GCNConv(input_dim, hidden_dim)
        self.conv2 = GCNConv(hidden_dim, output_dim)

    def forward(self, x, edge_index, edge_weight=None):
        x = F.dropout(x, p=0.4, training=self.training)
        x = self.conv1(x, edge_index, edge_weight=edge_weight)
        x = F.relu(x)
        x = F.dropout(x, p=0.4, training=self.training)
        x = self.conv2(x, edge_index, edge_weight=edge_weight)
        return x

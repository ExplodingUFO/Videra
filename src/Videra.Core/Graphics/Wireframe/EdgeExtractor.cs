namespace Videra.Core.Graphics.Wireframe;

/// <summary>
/// 从三角形网格中提取边缘线
/// </summary>
public static class EdgeExtractor
{
    /// <summary>
    /// 边缘结构（规范化存储，确保唯一性）
    /// </summary>
    public readonly struct Edge : IEquatable<Edge>
    {
        public readonly uint V0;
        public readonly uint V1;

        public Edge(uint a, uint b)
        {
            // 规范化：小的索引在前
            if (a < b) { V0 = a; V1 = b; }
            else { V0 = b; V1 = a; }
        }

        public bool Equals(Edge other) => V0 == other.V0 && V1 == other.V1;
        public override bool Equals(object? obj) => obj is Edge other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(V0, V1);
        public static bool operator ==(Edge left, Edge right) => left.Equals(right);
        public static bool operator !=(Edge left, Edge right) => !left.Equals(right);
    }

    /// <summary>
    /// 从三角形索引数组中提取唯一边缘
    /// </summary>
    /// <param name="triangleIndices">三角形索引数组（每3个索引一个三角形）</param>
    /// <returns>去重后的边缘线索引数组（每2个索引一条线）</returns>
    public static uint[] ExtractUniqueEdges(uint[] triangleIndices)
    {
        if (triangleIndices == null || triangleIndices.Length < 3)
            return Array.Empty<uint>();

        var uniqueEdges = new HashSet<Edge>();

        // 遍历每个三角形，提取3条边
        for (int i = 0; i < triangleIndices.Length; i += 3)
        {
            if (i + 2 >= triangleIndices.Length)
                break;

            uint v0 = triangleIndices[i];
            uint v1 = triangleIndices[i + 1];
            uint v2 = triangleIndices[i + 2];

            uniqueEdges.Add(new Edge(v0, v1));
            uniqueEdges.Add(new Edge(v1, v2));
            uniqueEdges.Add(new Edge(v2, v0));
        }

        // 转换为线索引数组
        var lineIndices = new uint[uniqueEdges.Count * 2];
        int idx = 0;
        foreach (var edge in uniqueEdges)
        {
            lineIndices[idx++] = edge.V0;
            lineIndices[idx++] = edge.V1;
        }

        return lineIndices;
    }

    /// <summary>
    /// 计算边缘数量（用于预估内存）
    /// 对于封闭流形网格：E = 3F/2（每条边被2个三角形共享）
    /// </summary>
    public static int EstimateEdgeCount(int triangleCount)
    {
        return (int)(triangleCount * 1.5); // 保守估计
    }
}

using GraphSearch.ConsoleApp.Constants;

namespace GraphSearch.ConsoleApp.Components;

public class VisibilityGraph : Graph
{
    public HashSet<Vertex> GetVisibleAssets(int vUserId)
    {
        var vAssets = new HashSet<Vertex>();
        var vUser = V(vUserId);
        var vOus = vUser.GetEdges().Select(e => e.Other(vUser)).ToArray();

        foreach (var vOu in vOus)
        {
            var vOuAssets = vOu.DFS(
                process: args =>
                {
                    var (v, e, s) = args;
                    if (v.Id == vOu.Id)
                        return (false, true, s);

                    var firstExclude = v.GetEdges().FirstOrDefault(e => e.Other(v).Id == vOu.Id && (e.Excluded || e.ExcludedAc));
                    var accepted = firstExclude == null;
                    var isExpanding = s.Equals(true);
                    var isApplyChildren = e.NormalAc;
                    var isNotExcludedAc = firstExclude?.ExcludedAc != true;
                    var shouldExpand = (isExpanding || isApplyChildren) && isNotExcludedAc;
                    return (accepted, shouldExpand, shouldExpand);
                },
                neighborFilter: v => v.Type == EVertexType.Asset,
                beforePop: null,
                initialState: false
            );

            foreach (var vAsset in vOuAssets)
                vAssets.Add(vAsset);
        }

        return vAssets;
    }

    public List<Vertex> GetFullAssetTree(HashSet<int> includedInTree, int vAssetId)
    {
        var vAsset = V(vAssetId);
        var tree = vAsset.BFS(
            process: args =>
            {
                var (v, _, _) = args;
                var isIncluded = includedInTree.Contains(v.Id);
                return (isIncluded, isIncluded, null);
            },
            neighborFilter: v => v.Type == EVertexType.Asset,
            initialState: null
        );
        return tree;
    }

    public (HashSet<int> IncludedInTree, HashSet<int> Unauthorized) CheckVisibility(int vUserId, int vAssetId)
    {
        var visibleAssets = GetVisibleAssets(vUserId);
        var vAsset = V(vAssetId);
        var unauthorized = new HashSet<int>();
        var includedInTree = new HashSet<int>();
        _ = vAsset.DFS(
            process: args =>
            {
                var (v, _, _) = args;
                var visible = visibleAssets.Contains(v);
                if (!visible)
                    unauthorized.Add(v.Id);

                return (false, true, visible);
            },
            neighborFilter: v => v.Type == EVertexType.Asset,
            beforePop: (v, _, s) =>
            {
                if (v.Parent is not null && (s.Equals(true) || includedInTree.Contains(v.Id)))
                {
                    includedInTree.Add(v.Id);
                    includedInTree.Add(v.Parent.Id);
                }
            },
            initialState: true
        );

        return (includedInTree, unauthorized);
    }

    public List<Vertex> GetFirstVisibleAssetTree(int vAssetId, HashSet<int> includedInTree, HashSet<int> unauthorized)
    {
        var vAsset = V(vAssetId);
        var found = false;
        var firstVisible = vAsset.BFS(
            process: args =>
            {
                if (found)
                    return (false, false, null);

                var (v, e, _) = args;
                if (v.Id == vAsset.Id)
                    return (false, true, null);

                var isIncluded = includedInTree.Contains(v.Id);
                found = isIncluded && !unauthorized.Contains(v.Id);

                return (found, !found, null);
            },
            neighborFilter: v => v.Type == EVertexType.Asset,
            initialState: null
        ).FirstOrDefault();

        if (firstVisible is not null)
        {
            var tree = new List<Vertex>() { vAsset };
            var currentPathAsset = firstVisible;
            while (currentPathAsset is not null && currentPathAsset.Parent?.Id != vAsset.Id)
            {
                tree.Add(currentPathAsset);
                currentPathAsset = currentPathAsset.Parent;
            }

            var directChildren = vAsset.GetEdges().Where(e => e.From.Id == vAsset.Id && e.IsTree && includedInTree.Contains(e.To.Id)).Select(e => e.To);
            tree.AddRange(directChildren);
            return tree;
        }

        return [vAsset];
    }
}

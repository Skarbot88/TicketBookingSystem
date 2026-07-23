# Optimistic approach 

Simply put my approach was to let both requests proceed, and make the write itself fail cleanly for the loser. This required us to process the entire reservation process using one atomic call. When we run `ExecuteUpdateAsync` it generates one piece of SQL afterwhich SQLite locks the entire database (not great for prod). 
# DelHardlinks
With this command-line tool for Windows, you can delete hardlinks, junctions and symlinks from files and folders.

________________________________________________________________________________
 DelHardlinks vs. 1.0.0.0 by (2019) R.Santos
 License: BSD (3-clause)
!!! Use this tool with caution.
If you don't know what it is, don't use it.
Improper use can result in data loss. !!!


 Usage: DelHardlinks [options] "full directory path"
 Options:
        -dH, delete hardlinks
        -dJ, delete all junctions
        -dK, delete all symbolic links
        -s, scan into subfolders
        -sL, scan into linked subfolders
        <Opt_DeleteEmptyDir>, <Opt_Str_DeleteEmptyDir>
        -p, permanent deletion
        -o, show full path
        -m, user iteration <press a key to halt before quitting>
        -y, assume "yes" to every deletion
        "-l:LOG", error log file. Where LOG is the full path of the log file
        -u:XX, maximum disk usage. Where XX is 20-90 in %
        Press <spacebar> key to pause and <esc> to abort

 Example: DelHardLinks -dH -s "f:\test\"

 Notes.
        When using the -dJ or -dK options, the -sL option is ignored.
        The -dH option deletes copies of files only. The last file found in the search tree will be kept.
        The -dJ and -dK options delete all links. Their targets will be kept

        Deleted items format:
 [H:hardlinks J:junctions S:symbolic links, F:scanned files/files in directory, E:error]

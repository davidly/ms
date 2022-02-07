//
// Text file sort app
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;
using System.Runtime.CompilerServices;

class MySort
{
    static void Usage()
    {
        Console.WriteLine( "Usage: ms [-c:X] [-i] [-l] [-n] [-r] [-s:X] [-u] [-w:X] inputfile outputfile" );
        Console.WriteLine( "  My Sort" );
        Console.WriteLine( "  arguments: inputfile   The file to sort. Text file with 8-bit ascii characters." );
        Console.WriteLine( "             outputfile  The resulting sorted file. Any existing file will be obliterated." );
        Console.WriteLine( "             -c:X        Sort starting on column X (0-based)" );
        Console.WriteLine( "             -i          Ignore case when sorting strings" );
        Console.WriteLine( "             -l          Sort based on line length only" );
        Console.WriteLine( "             -n          Sort numbers, not characters" );
        Console.WriteLine( "             -r          Reverse the sort (high to low)" );
        Console.WriteLine( "             -s:X        Line Separators on output: W (Windows CRLF. Default), M (Mac CR), U (Unix LF)" );
        Console.WriteLine( "             -u          Uniquify the output; remove duplicate adjacent lines" );
        Console.WriteLine( "             -w:X        Only include words of length X in the output" );
        Console.WriteLine( "  examples:  ms in.txt out.txt" );
        Console.WriteLine( "             ms -c:12 in.txt out.txt" );
        Console.WriteLine( "             ms -w:U -l -r in.txt out.txt" );

        Environment.Exit(1);
    } //Usage

    static int s_SortColumn = 0;
    static int s_WordLength = -1;
    static bool s_IgnoreCase = false;
    static bool s_SortOnLineLength = false;
    static bool s_SortByNumbers = false;
    static bool s_ReverseSort = false;
    static bool s_UniqueLines = false;
    static char s_LineSeparators = 'W';
    static byte [] s_In = null;
    static int [] s_LineOffsets = null;
    static char [] s_lineBuffer = null;

    const byte CR = 13;
    const byte LF = 10;

    static byte [] Win_CRLF = { CR, LF };
    static byte [] Mac_CR = { CR };
    static byte [] Unix_LF = { LF };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool CharOK( int offset )
    {
        if ( offset >= s_In.Length )
            return false;

        byte b = s_In[ offset ];

        return ( b != CR && b != LF );
    } //CharOK

    static int WordLength( int offset )
    {
        int l = 0;

        do
        {
            if ( offset >= s_In.Length )
                return l;

            byte b = s_In[ offset ];

            if ( b == CR || b == LF )
                return l;

            l++;
            offset++;
        } while ( true );
    } //WordLength

    public class MyComparer : IComparer
    {
        public int Compare( Object x, Object y )
        {
            int a = (int) x;
            int b = (int) y;

            // For some reason, the .net sort function calls Compare with the SAME value for both arguments

            if ( a == b )
                return 0;

            if ( s_ReverseSort )
            {
                b = (int) x;
                a = (int) y;
            }

            if ( 0 != s_SortColumn )
            {
                // don't walk past the end of the line or array

                for ( int i = 0; i < s_SortColumn; i++ )
                {
                    if ( !CharOK( a ) )
                        break;

                    a++;
                }

                for ( int i = 0; i < s_SortColumn; i++ )
                {
                    if ( !CharOK( b ) )
                        break;

                    b++;
                }
            }

            if ( s_SortOnLineLength )
            {
                int aLen = 0;
                while ( CharOK( a + aLen ) )
                    aLen++;

                int bLen = 0;
                while ( CharOK( b + bLen ) )
                    bLen++;

                return aLen - bLen;
            }

            if ( s_SortByNumbers )
            {
                // wow this code is slow. But does it matter?

                int aLen = 0;
                while ( CharOK( a + aLen ) )
                {
                    s_lineBuffer[ aLen ] = (char) s_In[ a + aLen ];
                    aLen++;
                }

                long ia = 0;

                if ( 0 != aLen )
                {
                    string sa = new string( s_lineBuffer, 0, aLen );
                    try { ia = Convert.ToInt64( sa ); } catch (Exception e) {}
                }

                int bLen = 0;
                while ( CharOK( b + bLen ) )
                {
                    s_lineBuffer[ bLen ] = (char) s_In[ b + bLen ];
                    bLen++;
                }

                long ib = 0;

                if ( 0 != bLen )
                {
                    string sb = new string( s_lineBuffer, 0, bLen );
                    try { ib = Convert.ToInt64( sb ); } catch (Exception e) {}
                }

                if ( ia > ib )
                    return 1;

                if ( ia < ib )
                    return -1;

                return 0;
            }

            if ( s_IgnoreCase )
            {
                while ( CharOK( a ) && CharOK( b ) )
                {
                    char ca = Char.ToLower( (char) s_In[ a ] );
                    char cb = Char.ToLower( (char) s_In[ b ] );

                    if ( ca > cb )
                        return 1;

                    if ( ca < cb )
                        return -1;

                    a++;
                    b++;
                }

                if ( !CharOK( a ) && !CharOK( b ) )
                    return 0;

                if ( CharOK( a ) )
                    return 1;

                return -1;
            }

            // normal compare

            while ( CharOK( a ) && CharOK( b ) )
            {
                byte ca = s_In[ a ];
                byte cb = s_In[ b ];

                if ( ca > cb )
                    return 1;

                if ( ca < cb )
                    return -1;

                a++;
                b++;
            }

            if ( !CharOK( a ) && !CharOK( b ) )
                return 0;

            if ( CharOK( a ) )
                return 1;

            return -1;
        } //Compare
    } //MyComparer

    public class HeapSort<T>
    {
         private T [] a;
         private int c;
         private System.Collections.IComparer comp;

         public HeapSort( T [] array, System.Collections.IComparer comparer )
         {
             a = array;
             c = array.Length;
             comp = comparer;
         } //HeapSort

         public void Sort()
         {
             if ( c < 2 )
                 return;

             for ( int i = ( ( c + 1 ) / 2 ) - 1; i >= 0; i-- )
                 AddRoot( i, c );

             for ( int j = c - 1; 0 != j; j-- )
             {
                 Swap( 0, j );
                 AddRoot( 0, j );
             }
         } //Sort

         private void Swap( int x, int y )
         {
             T tmp = a[ x ];
             a[ x ] = a[ y ];
             a[ y ] = tmp;
         } //Swap

         private void AddRoot( int x, int cItems )
         {
             int y = ( 2 * ( x + 1 ) ) - 1;

             while ( y < cItems )
             {
                 if ( ( y + 1 ) < cItems )
                 {
                     int i = comp.Compare( a[y], a[y + 1] );
                     if ( i < 0 )
                         y++;
                 }

                 int j = comp.Compare( a[x], a[y] );

                 if ( j < 0 )
                 {
                     Swap( x, y );
                     x = y;
                     y = ( 2 * ( y + 1 ) ) - 1;
                 }
                 else
                     break;
             }
         } //AddRoot
    } //HeapSort

    static bool ByteArraySame( byte [] a, int aLen, byte [] b, int bLen )
    {
        if ( aLen != bLen )
            return false;

        for ( int i = 0; i < aLen; i++ )
        {
            if ( a[i] != b[i] )
                return false;
        }

        return true;
    } //ByteArraySame

    static void Main( string[] args )
    {
        Stopwatch stopWatch = new Stopwatch();
        stopWatch.Start();

        string inFile = null;
        string outFile = null;

        for ( int i = 0; i < args.Length; i++ )
        {
            if ( '-' == args[i][0] || '/' == args[i][0] )
            {
                string argUpper = args[i].ToUpper();
                string arg = args[i];
                char c = argUpper[1];
    
                if ( 'C' == c )
                {
                    if ( ( arg.Length < 4 ) || ( ':' != arg[2] ) )
                        Usage();

                    s_SortColumn = Convert.ToInt32( arg.Substring( 3 ) );
                }
                else if ( 'I' == c )
                    s_IgnoreCase = true;
                else if ( 'L' == c )
                    s_SortOnLineLength = true;
                else if ( 'N' == c )
                    s_SortByNumbers = true;
                else if ( 'R' == c )
                    s_ReverseSort = true;
                else if ( 'S' == c )
                {
                    if ( ( arg.Length != 4 ) || ( ':' != arg[2] ) )
                        Usage();

                    char s = argUpper[3];

                    if ( s != 'W' && s != 'M' && s != 'U' )
                        Usage();

                    s_LineSeparators = s;
                }
                else if ( 'U' == c )
                    s_UniqueLines = true;
                else if ( 'W' == c )
                {
                    if ( ( arg.Length < 4 ) || ( ':' != arg[2] ) )
                        Usage();

                    s_WordLength = Convert.ToInt32( arg.Substring( 3 ) );
                }
                else
                    Usage();
            }
            else
            {
                if ( inFile == null )
                    inFile = args[i];
                else if ( outFile == null )
                    outFile = args[i];
                else
                    Usage();
            }
        }
    
        if ( ( null == inFile ) || ( null == outFile ) )
            Usage();
    
        if ( ! File.Exists( inFile ) )
        {
            Console.WriteLine( "can't find input file {0}", inFile );
            Environment.Exit(1);
        }

        try
        {
            // I looked at using a mapped file instead of reading this all into RAM, but
            // the .net APIs are awkward, pointers don't work well in C#, and machines have
            // a lot more RAM than in 1993 when I wrote the C++ version using mapped files.

            s_In = File.ReadAllBytes( inFile );

            // A line can end with one of:
            //    cr/lf (13/10) Windows
            //    cr (13) MacOS through version 9
            //    lf (10) Unix, Linux, modern MacOS
            //    EOF All of the above
            //
            // Ignore empty lines.
            //

            // Count the number of lines.

            int lines = 0;
            int i = 0;

            do
            {
                // Get past line separators

                while ( ( i < s_In.Length ) &&
                        ( ( s_In[i] == CR ) || ( s_In[i] == LF ) ) )
                    i++;

                if ( i == s_In.Length )
                    break;

                // Read the next line

                while ( CharOK( i ) )
                    i++;

                lines++;
            } while ( true );

            // Record the offset of the start of each non-0-length line

            s_LineOffsets = new int[ lines ];
            int line = 0;
            int longestLine = 0;
            i = 0;

            do
            {
                // Get past line separators

                while ( ( i < s_In.Length ) &&
                        ( ( s_In[i] == CR ) || ( s_In[i] == LF ) ) )
                    i++;

                if ( i == s_In.Length )
                    break;

                if ( -1 == s_WordLength || s_WordLength == WordLength( i ) )
                {
                    // Read the next line

                    int start = s_LineOffsets[ line++ ] = i;

                    while ( CharOK( i ) )
                        i++;

                    int len = i - start;
                    if ( len > longestLine )
                        longestLine = len;
                }
                else
                {
                    // skip the word;

                    while ( CharOK( i ) )
                        i++;
                }
            } while ( true );

            // in case lines were skipped due to word length, resize the array

            lines = line;
            Array.Resize( ref s_LineOffsets, lines );

            long afterReadTime = stopWatch.ElapsedMilliseconds;

            //Console.WriteLine( "sorting file of size {0} with {1} lines and longest line {2}", s_In.Length, lines, longestLine );

            // At this point, s_LineOffsets has indexes into s_In at the start of each non-0-length line.
            // Each line ends with one of a CR, LF, or end of file (end of the s_In buffer).

            s_lineBuffer = new char[ longestLine ];
            IComparer cmp = new MyComparer();

            #if true // The heapsort is generally much faster than the built-in Array.Sort

                HeapSort<int> hs = new HeapSort<int>( s_LineOffsets, cmp );

                hs.Sort();

            #else

                Array.Sort( s_LineOffsets, cmp );

            #endif

            long afterSortTime = stopWatch.ElapsedMilliseconds;

            byte [] curLine = new byte[ longestLine ];
            byte [] prevLine = new byte[ longestLine ];
            int prevLen = 0;

            byte [] separator = null;

            if ( s_LineSeparators == 'W' )
                separator = Win_CRLF;
            else if ( s_LineSeparators == 'M' )
                separator = Mac_CR;
            else if ( s_LineSeparators == 'U' )
                separator = Unix_LF;
            else
                Usage(); // should never get here...

            FileStream fsOut = new FileStream( outFile, FileMode.Create );

            for ( line = 0; line < lines; line++ )
            {
                int o = s_LineOffsets[ line ];
                int z = 0;

                while ( CharOK( o ) )
                    curLine[z++] = s_In[o++];

                if ( ! ( s_UniqueLines && ByteArraySame( curLine, z, prevLine, prevLen ) ) )
                {
                    fsOut.Write( curLine, 0, z );

                    fsOut.Write( separator, 0, separator.Length );

                    curLine.CopyTo( prevLine, 0 );
                    prevLen = z;
                }
            }

            fsOut.Flush();
            fsOut.Dispose();

            long endTime = stopWatch.ElapsedMilliseconds;

            Console.WriteLine( "completed in {0,8:N0} millisconds. {1,8:N0} reading, {2,8:N0} sorting, {3,8:N0} writing",
                               endTime, afterReadTime, afterSortTime - afterReadTime, endTime - afterSortTime  );
        }
        catch (Exception e)
        {
            Console.WriteLine( "ms.exe caught an exception {0}", e.ToString() );
            Usage();
        }
    } //Main
} //MySort


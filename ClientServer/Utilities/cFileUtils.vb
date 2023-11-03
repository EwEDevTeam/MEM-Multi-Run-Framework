' ===============================================================================
' This file is part of the Marine Ecosystem Model multi-run framework prototype, 
' developed for the PhD thesis of Jeroen Steenbeek (2020-2023) in Marine Sciences 
' at the Polytechnical University of Catalunya.
'
' The MEM run framework is distributed in the hope that it will be useful, but 
' WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or 
' FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more 
' details.
'
' You should have received a copy of the GNU General Public License along with EwE.
' If not, see <http:'www.gnu.org/licenses/gpl-2.0.html>. 
'
' Copyright 2020- 
'    Ecopath International Initiative, Barcelona, Spain
' ==============================================================================='

#Region " Imports "

Option Strict On
Imports System.IO
Imports System.Runtime.InteropServices

#End Region ' Imports

Public Class cFileUtils

    Private Const ERROR_SHARING_VIOLATION As Integer = 32
    Private Const ERROR_LOCK_VIOLATION As Integer = 33

    Public Shared Function FixPathCharacters(pin As String) As String

        If (Path.PathSeparator = Path.DirectorySeparatorChar) Then
            Return pin.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar)
        End If
        Return pin.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)

    End Function


    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Utility method; determines whether the specified file is not locked.
    ''' </summary>
    ''' <param name="fin">The file to check.</param>
    ''' <returns>
    '''   <c>true</c> if the specified file is ready.
    ''' </returns>
    ''' -----------------------------------------------------------------------
    Public Shared Function IsFileReady(fin As String) As Boolean
        Try
            Using fs As FileStream = File.Open(fin, FileMode.Open, FileAccess.ReadWrite, FileShare.None)
                If (fs IsNot Nothing) Then fs.Close() ' Bordering paranoia
                Return True
            End Using
        Catch ex As Exception
            ' The above could fail on legitimate read-only files
            If Not cFileUtils.IsFileLocked(ex) Then Return True
        Finally
            ' NOP
        End Try
        Return False
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Utility method, determines if the exception error code indicates a file lock.
    ''' </summary>
    ''' <param name="ex">The ex.</param>
    ''' <returns>
    '''   <c>true</c> if [is file locked] [the specified ex]; otherwise, <c>false</c>.
    ''' </returns>
    ''' -----------------------------------------------------------------------
    Private Shared Function IsFileLocked(ex As Exception) As Boolean
        Dim errorCode As Integer = Marshal.GetHRForException(ex) And ((1 << 16) - 1)
        Return errorCode = ERROR_SHARING_VIOLATION Or errorCode = ERROR_LOCK_VIOLATION
    End Function

    Private Enum eCaseSensitive As Integer
        NotDetermined = 0
        CaseSensitive = 1
        NotCaseSensitive = 2
    End Enum

    Private Shared s_casesensitive As eCaseSensitive = eCaseSensitive.NotDetermined

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns whether the current OS is (most likely) case sensitive
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>
    ''' https://stackoverflow.com/questions/430256/how-do-i-determine-whether-the-filesystem-is-case-sensitive-in-net
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Public Shared Function IsOSCaseSensitive() As Boolean

        If (s_casesensitive = eCaseSensitive.NotDetermined) Then
            '    If RuntimeInformation.IsOSPlatform(OSPlatform.Windows) Or RuntimeInformation.IsOSPlatform(OSPlatform.OSX) Then
            '        Return False
            '    ElseIf RuntimeInformation.IsOSPlatform(OSPlatform.Linux) Then
            '        Return True
            '    ElseIf Environment.OSVersion.Platform = PlatformID.Unix Then
            '        Return True
            '    Else
            '        Return False
            'End If

            Dim t As String = Path.GetTempPath()
            If Not Directory.Exists(t) Then t = Environment.CurrentDirectory

            If (Directory.Exists(t.ToLower) And Directory.Exists(t.ToUpper)) Then
                s_casesensitive = eCaseSensitive.NotCaseSensitive
            Else
                s_casesensitive = eCaseSensitive.CaseSensitive
            End If
        End If

        Debug.Assert(s_casesensitive <> eCaseSensitive.NotDetermined)
        Return (s_casesensitive = eCaseSensitive.CaseSensitive)

    End Function

    Public Shared Function FilesEqual(f1 As String, f2 As String) As Boolean

        If String.IsNullOrWhiteSpace(f1) Or String.IsNullOrWhiteSpace(f2) Then Return False
        f1 = Path.GetFullPath(f1)
        f2 = Path.GetFullPath(f2)
        Return (String.Compare(f1, f2, Not IsOSCaseSensitive()) = 0)

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns whether a file path is contained within a folder or folder structure. 
    ''' This method honours system file name case sensitivity. Note that this
    ''' method does not check whether the file actually exists.
    ''' </summary>
    ''' <param name="file">The file to check.</param>
    ''' <param name="bIncludeSubFolders"></param>
    ''' -----------------------------------------------------------------------
    Public Shared Function IsContained(file As String, folder As String, bIncludeSubFolders As Boolean) As Boolean

        file = Path.GetFullPath(file)
        Dim fld As String = folder

        If (Not cFileUtils.IsOSCaseSensitive()) Then
            file = file.ToLower()
            fld = fld.ToLower()
        End If

        If (bIncludeSubFolders) Then
            If Not Path.EndsInDirectorySeparator(fld) Then
                fld &= Path.DirectorySeparatorChar
            End If
            Return file.StartsWith(fld)
        Else
            Return Path.GetDirectoryName(file) = fld
        End If

    End Function

    Public Shared Function IsDirectory(f As String) As Boolean
        Return Directory.Exists(f)
    End Function

End Class

﻿' ===============================================================================
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

Public Class cFolderContentWatcherEventArgs
    Inherits EventArgs

    Public Sub New(file As String, change As cFolderContentWatcher.eChangeType)
        Me.File = file
        Me.Change = change
    End Sub

    Public ReadOnly Property File As String
    Public ReadOnly Property Change As cFolderContentWatcher.eChangeType

End Class

﻿using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using OpenBullet2.Entities;
using OpenBullet2.Helpers;
using OpenBullet2.Repositories;
using OpenBullet2.Shared.Forms;
using Radzen.Blazor;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenBullet2.Pages
{
    public partial class Wordlists
    {
        [Inject] IModalService Modal { get; set; }
        [Inject] IWordlistRepository WordlistRepo { get; set; }

        private List<WordlistEntity> wordlists;
        private WordlistEntity selectedWordlist;

        RadzenGrid<WordlistEntity> wordlistsGrid;
        private int resultsPerPage = 15;

        protected override async Task OnInitializedAsync()
        {
            await RefreshList();

            await base.OnInitializedAsync();
        }

        private async Task OnResultsPerPageChanged(int value)
        {
            resultsPerPage = value;
            await RefreshList();
            StateHasChanged();
        }

        private void SelectWordlist(WordlistEntity wordlist)
        {
            selectedWordlist = wordlist;
        }

        private async Task RefreshList()
        {
            wordlists = await WordlistRepo.GetAll().ToListAsync();

            StateHasChanged();
        }

        private async Task AddWordlist()
        {
            var modal = Modal.Show<WordlistAdd>(Loc["AddWordlist"]);
            var result = await modal.Result;

            if (!result.Cancelled)
            {
                wordlists.Add(result.Data as WordlistEntity);
                await js.AlertSuccess(Loc["Added"], Loc["AddedWordlist"]);
            }
        }

        private async Task EditWordlist()
        {
            if (selectedWordlist == null)
            {
                await ShowNoWordlistSelectedWarning();
                return;
            }

            var parameters = new ModalParameters();
            parameters.Add(nameof(WordlistEdit.Wordlist), selectedWordlist);

            var modal = Modal.Show<WordlistEdit>(Loc["EditWordlist"], parameters);
            await modal.Result;
        }

        private async Task DeleteWordlist()
        {
            if (selectedWordlist == null)
            {
                await ShowNoWordlistSelectedWarning();
                return;
            }

            if (await js.Confirm(Loc["AreYouSure"], $"{Loc["ReallyDelete"]} {selectedWordlist.Name}?"))
            {
                var deleteFile = await js.Confirm(Loc["AlsoDeleteFile"], 
                    $"{Loc["DeleteFileText1"]} {selectedWordlist.FileName} {Loc["DeleteFileText2"]}", Loc["KeepFile"]);

                // Delete the wordlist from the DB and disk
                await WordlistRepo.Delete(selectedWordlist, deleteFile);

                // Delete the wordlist from the local list
                wordlists.Remove(selectedWordlist);
            }
        }

        private async Task ShowNoWordlistSelectedWarning()
            => await js.AlertError(Loc["Uh-Oh"], Loc["NoWordlistSelected"]);
    }
}

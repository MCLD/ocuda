javascript:(function() {
    var elem = document.querySelector('[id^="bib-record-index"]');
    if(elem === null) { 
	    elem = document.querySelector('[id^="itemNameDisplay-"]').id; 
	    let pid = elem.split("itemNameDisplay-")[1]; 
	    let bibId = document.getElementById("bib-" + pid).children[0].text.trim(); 
	    window.open("{0}" + bibId.toString()); 
    } 
    else { 
	    let elemArr = elem.id.split("-"); 
	    let bibId = elemArr[elemArr.length-1]; 
	    window.open("{0}" + bibId.toString()); 
    }
})();
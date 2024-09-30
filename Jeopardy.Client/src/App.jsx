import { useEffect, useState } from 'react';
import './App.css';

function App() {
    const QUESTIONS_PER_CATEGORY = 5;
    const [gameboard, setGameboard] = useState();

    useEffect(() => {
        populateGameboard();
    }, []);

    let keys = !gameboard ? undefined : Object.keys(gameboard);
    let table = !gameboard ? undefined :
        <table>
            <thead>
                <tr>
                    {keys.map(key =>
                        <th key={key}>{key}</th>
                    )}
                </tr>
            </thead>
            <tbody>
                {[...Array(QUESTIONS_PER_CATEGORY).keys()].map(y =>
                    <tr key={y}>
                        {[...Array(keys.length).keys()].map(x =>
                            <td key={x} id={`${x}-${y}`} onClick={flip.bind(this, gameboard[keys[x]][y])}>{(y + 1) * 100}</td>
                        )}
                    </tr>
                )}
            </tbody>
        </table>;
        
    return (
        <div>
            {table}
        </div>
    );

    function flip(card, e) {
        if (card.state === undefined) card.state = 0
        card.state++;
        if (card.state == 1)
            e.target.innerHTML = card.question;
        else
            e.target.innerHTML = card.answer;
    }

    async function populateGameboard() {
        let a = window.location.href.slice(location.href.lastIndexOf("/"), location.href.length);
        const response = await fetch('gameboard/create' + a);
        const data = await response.json();
        setGameboard(data);
    }
}

export default App;
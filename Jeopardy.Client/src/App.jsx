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
                            <td key={x} id={`${x}-${y}`} onClick={flip.bind(this, gameboard[keys[x]][y])}>{gameboard[keys[x]][y].points}</td>
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
        card.state++;
        if (card.state == 1)
            e.target.innerText = card.question.question;
        else
            e.target.innerText = card.question.answer;
    }

    async function populateGameboard() {
        const response = await fetch('gameboard/create');
        const data = await response.json();
        setGameboard(data);
    }
}

export default App;